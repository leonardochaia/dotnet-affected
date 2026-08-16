namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// What the working tree contributes to the comparison, on top of the commits between the
    /// 'from' ref and the commit that is checked out.
    /// </summary>
    public enum UncommittedChanges
    {
        /// <summary>
        /// Staged and unstaged changes both count, including files git does not track yet.
        /// </summary>
        All = 0,

        /// <summary>
        /// Staged changes count, unstaged ones do not. What a pre-commit hook wants: the
        /// comparison describes the commit that is about to be made.
        /// </summary>
        Staged = 1,

        /// <summary>
        /// Neither counts. The comparison is between commits, and a dirty working tree makes
        /// no difference to it.
        /// </summary>
        None = 2,
    }
}
