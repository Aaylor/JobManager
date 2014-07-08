using System;

namespace JobManager.Exceptions {

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
    
}
