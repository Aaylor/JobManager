namespace JobManager.Job {
    
    public interface IJob {

        bool IsRepeatable();

        void Execute();
    
    }

}
