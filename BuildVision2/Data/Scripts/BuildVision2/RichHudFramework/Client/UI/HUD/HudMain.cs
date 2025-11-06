using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using FloatProp = VRage.MyTuple<System.Func<float>, System.Action<float>>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using RichStringMembers = VRage.MyTuple<System.Text.StringBuilder, VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>>;
using Vec2Prop = VRage.MyTuple<System.Func<VRageMath.Vector2>, System.Action<VRageMath.Vector2>>;

namespace RichHudFramework
{
	using Client;
	using Internal;
	using CursorMembers = MyTuple<
		Func<HudSpaceDelegate, bool>, // IsCapturingSpace
		Func<float, HudSpaceDelegate, bool>, // TryCaptureHudSpace
		Func<ApiMemberAccessor, bool>, // IsCapturing
		Func<ApiMemberAccessor, bool>, // TryCapture
		Func<ApiMemberAccessor, bool>, // TryRelease
		ApiMemberAccessor // GetOrSetMember
	>;
	using TextBuilderMembers = MyTuple<
		MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
		Func<Vector2I, int, object>, // GetCharMember
		ApiMemberAccessor, // GetOrSetMember
		Action<IList<RichStringMembers>, Vector2I>, // Insert
		Action<IList<RichStringMembers>>, // SetText
		Action // Clear
	>;

	namespace UI
	{
		using static NodeConfigIndices;
		using TextBoardMembers = MyTuple<
			TextBuilderMembers,
			FloatProp, // Scale
			Func<Vector2>, // Size
			Func<Vector2>, // TextSize
			Vec2Prop, // FixedSize
			Action<BoundingBox2, BoundingBox2, MatrixD[]> // Draw 
		>;

		namespace Client
		{
			using HudClientMembers = MyTuple<
				CursorMembers, // Cursor
				Func<TextBoardMembers>, // GetNewTextBoard
				ApiMemberAccessor, // GetOrSetMembers
				Action // Unregister
			>;


			public sealed partial class HudMain : RichHudClient.ApiModule<HudClientMembers>
			{
				/// <summary>
				/// Root parent for all HUD elements.
				/// </summary>
				public static HudParentBase Root
				{
					get
					{
						if (Instance == null)
							Init();

						return Instance._root;
					}
				}

				/// <summary>
				/// Root node for high DPI scaling at > 1080p. Draw matrix automatically rescales to comensate
				/// for decrease in apparent size due to high DPI displays.
				/// </summary>
				public static HudParentBase HighDpiRoot
				{
					get
					{
						if (Instance == null)
							Init();

						return Instance.highDpiRoot;
					}
				}

				/// <summary>
				/// Cursor shared between mods.
				/// </summary>
				public static ICursor Cursor
				{
					get
					{
						if (Instance == null)
							Init();

						return Instance.cursor;
					}
				}

				/// <summary>
				/// Shared clipboard.
				/// </summary>
				public static RichText ClipBoard
				{
					get
					{
						if (Instance == null)
							Init();

						object value = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.ClipBoard);

						if (value != null)
							return new RichText(value as List<RichStringMembers>);
						else
							return default(RichText);
					}
					set 
					{
						if (Instance == null)
							Init();

						Instance.GetOrSetMemberFunc(value.apiData, (int)HudMainAccessors.ClipBoard); 
					}
				}

				/// <summary>
				/// Resolution scale normalized to 1080p, for resolutions over 1080p. Returns a scale of 1f
				/// for lower resolutions.
				/// </summary>
				public static float ResScale { get; private set; }

				/// <summary>
				/// Matrix used to convert from 2D screen space coordinates in pixels to 3D worldspace in meters.
				/// </summary>
				public static MatrixD PixelToWorld => PixelToWorldRef[0];

				/// <summary>
				/// Matrix used to convert from 2D screen space coordinates in pixels to 3D worldspace in meters.
				/// </summary>
				public static MatrixD[] PixelToWorldRef { get; private set; }

				/// <summary>
				/// Current horizontal screen resolution in pixels.
				/// </summary>
				public static float ScreenWidth { get; private set; }

				/// <summary>
				/// Current vertical resolution in pixels.
				/// </summary>
				public static float ScreenHeight { get; private set; }

				/// <summary>
				/// Current screen dimensions ScreenWidth x ScreenHeight in pixels
				/// </summary>
				public static Vector2 ScreenDim { get; private set; }

				/// <summary>
				/// Current screen dimensions ScreenWidth x ScreenHeight with high DPI scaling
				/// </summary>
				public static Vector2 ScreenDimHighDPI { get; private set; }

				/// <summary>
				/// Current aspect ratio (ScreenWidth/ScreenHeight).
				/// </summary>
				public static float AspectRatio { get; private set; }

				/// <summary>
				/// Current field of view
				/// </summary>
				public static float Fov { get; private set; }

				/// <summary>
				/// Scaling used by MatBoards to compensate for changes in apparent size and position as a result
				/// of changes to Fov.
				/// </summary>
				public static float FovScale { get; private set; }

				/// <summary>
				/// The current opacity for the in-game menus as configured.
				/// </summary>
				public static float UiBkOpacity { get; private set; }

				/// <summary>
				/// Enables the cursor and appropriate input mode
				/// </summary>
				public static bool EnableCursor { get; set; }

				/// <summary>
				/// Current input mode. Used to indicate whether UI elements should accept cursor or text input.
				/// </summary>
				public static HudInputMode InputMode { get; private set; }

				public static HudMain Instance { get; private set; }

				public readonly HudParentBase _root;
				private readonly ScaledSpaceNode highDpiRoot;
				private readonly HudCursor cursor;
				private bool enableCursorLast;

				private readonly Func<TextBoardMembers> GetTextBoardDataFunc;
				private readonly ApiMemberAccessor GetOrSetMemberFunc;
				private readonly Action UnregisterAction;

				private HudMain() : base(ApiModuleTypes.HudMain, false, true)
				{
					if (Instance != null)
						throw new Exception("Only one instance of HudMain can exist at any give time!");

					Instance = this;
					var members = GetApiData();

					cursor = new HudCursor(members.Item1);
					GetTextBoardDataFunc = members.Item2;
					GetOrSetMemberFunc = members.Item3;
					UnregisterAction = members.Item4;

					PixelToWorldRef = new MatrixD[1];
					_root = new HudClientRoot();
					highDpiRoot = new ScaledSpaceNode(_root) { UpdateScaleFunc = () => ResScale };

					// Register update handle
					GetOrSetMemberFunc(_root.DataHandle, (int)HudMainAccessors.ClientRootNode);
					GetOrSetMemberFunc(new Action(() => ExceptionHandler.Run(BeforeMasterDraw)), (int)HudMainAccessors.SetBeforeDrawCallback);

					UpdateCache();
				}

				public static void Init()
				{
					BillBoardUtils.Init();

					if (Instance == null)
						new HudMain();
				}

				private void BeforeMasterDraw()
				{
					UpdateCache();
					cursor.Update();
				}

				public override void Close()
				{
					UnregisterAction?.Invoke();
					Instance = null;
				}

				private void UpdateCache()
				{
					ScreenWidth = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenWidth);
					ScreenHeight = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ScreenHeight);
					AspectRatio = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.AspectRatio);
					ResScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.ResScale);
					Fov = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.Fov);
					FovScale = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.FovScale);
					PixelToWorldRef[0] = (MatrixD)GetOrSetMemberFunc(null, (int)HudMainAccessors.PixelToWorldTransform);
					UiBkOpacity = (float)GetOrSetMemberFunc(null, (int)HudMainAccessors.UiBkOpacity);
					InputMode = (HudInputMode)GetOrSetMemberFunc(null, (int)HudMainAccessors.InputMode);

					ScreenDim = new Vector2(ScreenWidth, ScreenHeight);
					ScreenDimHighDPI = ScreenDim / ResScale;

					if (EnableCursor != enableCursorLast)
						GetOrSetMemberFunc(EnableCursor, (int)HudMainAccessors.EnableCursor);
					else
						EnableCursor = (bool)GetOrSetMemberFunc(null, (int)HudMainAccessors.EnableCursor);

					enableCursorLast = EnableCursor;
				}

				/// <summary>
				/// Returns the ZOffset for focusing a window and registers a callback
				/// for when another object takes focus.
				/// </summary>
				public static byte GetFocusOffset(Action<byte> LoseFocusCallback)
				{
					if (Instance == null)
						Init();

					return (byte)Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetFocusOffset);
				}

				/// <summary>
				/// Registers a callback for UI elements taking input focus. Callback
				/// invoked when another element takes focus.
				/// </summary>
				public static void GetInputFocus(Action LoseFocusCallback)
				{
					if (Instance == null)
						Init();

					Instance.GetOrSetMemberFunc(LoseFocusCallback, (int)HudMainAccessors.GetInputFocus);
				}

				/// <summary>
				/// Returns accessors for a new TextBoard
				/// </summary>
				public static TextBoardMembers GetTextBoardData() 
				{
					if (Instance == null)
						Init();

					return Instance.GetTextBoardDataFunc();
				}
					

				/// <summary>
				/// Converts from a vector in normalized units to pixels.
				/// </summary>
				public static Vector2 GetPixelVector(Vector2 scaledVec)
				{
					if (Instance == null)
						Init();

					return new Vector2
					(
						(int)(scaledVec.X * ScreenWidth),
						(int)(scaledVec.Y * ScreenHeight)
					);
				}

				/// <summary>
				/// Converts from a vector in pixels to normalized units
				/// </summary>
				public static Vector2 GetAbsoluteVector(Vector2 pixelVec)
				{
					if (Instance == null)
						Init();

					return new Vector2
					(
						pixelVec.X / ScreenWidth,
						pixelVec.Y / ScreenHeight
					);
				}

				/// <summary>
				/// Root UI element for the client
				/// </summary>
				private class HudClientRoot : HudParentBase, IReadOnlyHudSpaceNode
				{
					public bool DrawCursorInHudSpace { get; }

					public Vector3 CursorPos { get; private set; }

					public HudSpaceDelegate GetHudSpaceFunc { get; }

					public MatrixD PlaneToWorld => PlaneToWorldRef[0];

					public MatrixD[] PlaneToWorldRef { get; }

					public Func<MatrixD> UpdateMatrixFunc { get; }

					public Func<Vector3D> GetNodeOriginFunc
					{
						get { return DataHandle[0].Item2[0]; }
						private set { DataHandle[0].Item2[0] = value; }
					}

					public bool IsInFront { get; }

					public bool IsFacingCamera { get; }

					public HudClientRoot()
					{
						DrawCursorInHudSpace = true;
						HudSpace = this;
						IsInFront = true;
						IsFacingCamera = true;
						PlaneToWorldRef = PixelToWorldRef;

						GetHudSpaceFunc = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceFunc) as HudSpaceDelegate;
						GetNodeOriginFunc = Instance.GetOrSetMemberFunc(null, (int)HudMainAccessors.GetPixelSpaceOriginFunc) as Func<Vector3D>;
						Config[StateID] |= (uint)(HudElementStates.CanUseCursor | HudElementStates.IsSpaceNode);
					}

					protected override void Layout()
					{
						CursorPos = new Vector3(Cursor.ScreenPos.X, Cursor.ScreenPos.Y, 0f);
					}
				}
			}
		}
	}

	namespace UI.Server
	{ }
}