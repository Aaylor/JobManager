namespace JobManager.Job {

    public abstract class Work : IJob {
    
        public abstract bool IsRepeatable();

        public void Execute() {
        
        }

    }



    public abstract class RepeatableWork : Work {
    
        public sealed override bool IsRepeatable() {
            return true; 
        }

    }



    public abstract class UniqueWork : Work {
    
        public sealed override bool IsRepeatable() {
            return false; 
        }

    }
    
}
