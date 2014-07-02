namespace JobManager.Job {

    public abstract class Task : IJob {

        public abstract bool IsRepeatable();

        public void Execute() {
        
        }
    
    }



    public abstract class RepeatableTask : Task {
    
        public sealed override bool IsRepeatable() {
            return true;
        }

    }



    public abstract class UniqueTask : Task {

        public sealed override bool IsRepeatable() {
            return false; 
        }
    
    }
    
}
