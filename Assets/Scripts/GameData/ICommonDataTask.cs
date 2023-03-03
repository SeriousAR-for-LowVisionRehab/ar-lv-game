/// <summary>
/// Common data among tasks
/// </summary>
interface ICommonDataTask
{
    public string TaskID { get; set; }    
    public bool IsSolved { get; set; }
    public float TaskDuration { get; set; }
}
