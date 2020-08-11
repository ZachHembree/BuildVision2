namespace RichHudFramework.UI
{
    public enum BindClientAccessors : int
    {
        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetOrCreateGroup = 1,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetBindGroup = 2,

        /// <summary>
        /// in: IReadOnlyList{string}, out: int[]
        /// </summary>
        GetComboIndices = 3,

        /// <summary>
        /// in: string, out: int
        /// </summary>
        GetControlByName = 4,

        /// <summary>
        /// void
        /// </summary>
        ClearBindGroups = 5,

        /// <summary>
        /// void
        /// </summary>
        Unload = 6,
    }
}