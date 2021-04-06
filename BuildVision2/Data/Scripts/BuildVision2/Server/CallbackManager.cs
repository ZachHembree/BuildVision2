using System;
using System.Collections.Generic;
using VRage;
using VRage.Utils;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvServer
    {
        private class CallbackManager
        {
            private readonly Dictionary<int, Action<byte[]>> callbackMap;
            private List<MyTuple<int, Action<byte[]>>> callbackList, callbackBuffer;

            private readonly HashSet<int> idExcludeSet;
            private List<int> idList, idBuffer;

            public CallbackManager()
            {
                callbackMap = new Dictionary<int, Action<byte[]>>();

                callbackList = new List<MyTuple<int, Action<byte[]>>>();
                callbackBuffer = new List<MyTuple<int, Action<byte[]>>>();

                idExcludeSet = new HashSet<int>();
                idList = new List<int>();
                idBuffer = new List<int>();
            }

            /// <summary>
            /// Registers a callback delegate for server replies and returns the ID. ID == -1 if
            /// not unique and already registered.
            /// </summary>
            public int RegisterCallback(Action<byte[]> callback, bool unique = true)
            {
                if (!unique || !callbackMap.ContainsValue(callback))
                {
                    var callbackEntry = new MyTuple<int, Action<byte[]>>(GetNewID(), callback);

                    callbackList.Add(callbackEntry);
                    callbackMap.Add(callbackEntry.Item1, callbackEntry.Item2);

                    return callbackEntry.Item1;
                }
                else
                    return -1;
            }

            /// <summary>
            /// Returns a new unique int IDs for callback delegates
            /// </summary>
            private int GetNewID()
            {
                int nextID = (idList.Count > 0 ? idList[idList.Count - 1] : -1) + 1;
                idList.Add(nextID);

                return nextID;
            }

            /// <summary>
            /// Invokes callbacks associated with the IDs in the list and passes in the server's serialized reply.
            /// </summary>
            public void InvokeCallbacks(IReadOnlyList<ServerReplyMessage> replyData)
            {
                idExcludeSet.Clear();

                for (int i = 0; i < replyData.Count; i++)
                    idExcludeSet.Add(replyData[i].callbackID);

                // Build ID list without the IDs of the callbacks being invoked
                idBuffer.Clear();
                idBuffer.EnsureCapacity(idList.Count);

                for (int i = 0; i < idList.Count; i++)
                {
                    if (!idExcludeSet.Contains(idList[i]))
                        idBuffer.Add(idList[i]);
                }

                // Build callback list without the callbacks being invoked
                callbackBuffer.Clear();
                callbackBuffer.EnsureCapacity(callbackList.Count);

                for (int i = 0; i < callbackList.Count; i++)
                {
                    if (!idExcludeSet.Contains(idList[i]))
                        callbackBuffer.Add(callbackList[i]);
                }

                // Clean up old list and swap buffer lists
                callbackList.Clear();
                idList.Clear();

                MyUtils.Swap(ref idList, ref idBuffer);
                MyUtils.Swap(ref callbackList, ref callbackBuffer);

                idList.Sort();

                // Invoke callback delegates and rebuild map
                foreach (var entry in replyData)
                    callbackMap[entry.callbackID](entry.data);

                callbackMap.Clear();

                foreach (var entry in callbackList)
                    callbackMap.Add(entry.Item1, entry.Item2);
            }
        }
    }
}
