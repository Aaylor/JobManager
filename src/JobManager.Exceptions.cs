using System;

namespace JobManager.Exceptions {

    /// <summary>
    /// Raised when a json data is incorrect.
    /// </summary>
    public class InvalidObjectException : Exception {

        public InvalidObjectException() {
        
        }

        public InvalidObjectException(string message)
            : base(message) {
            
        }

        public InvalidObjectException(string message, Exception inner)
            : base(message, inner) {
            
        }
    
    }



    /// <summary>
    /// Raised when a job execution has returned 0.
    /// </summary>
    public class BadReturnedValueException : Exception {
    
        public BadReturnedValueException() {
        
        }

        public BadReturnedValueException(string message)
            : base(message) {
            
        }

        public BadReturnedValueException(string message, Exception inner)
            : base(message, inner) {
            
        }
        
    }



    /// <summary>
    /// Raised when a job throw an exception.
    /// </summary>
    public class ExecutionException : Exception {
    
        public ExecutionException() {
        
        }

        public ExecutionException(string message)
            : base(message) {
            
        }

        public ExecutionException(string message, Exception inner)
            : base(message, inner) {
            
        }
        
    }
    


    /// <summary>
    /// Raised when a job already exists in the list.
    /// </summary>
    public class JobAlreadyExistsException : Exception {
    
        public JobAlreadyExistsException() {
        
        }

        public JobAlreadyExistsException(string message)
            : base(message) {
            
        }

        public JobAlreadyExistsException(string message, Exception inner)
            : base(message, inner) {
            
        }
        
    }
}
