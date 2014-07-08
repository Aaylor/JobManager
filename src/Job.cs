using JobManager;
using JobManager.Date;
using JobManager.Exceptions;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JobManager.Job {
   
    public abstract class Job {

        internal Manager _parentManager;
        internal object _referenceToJobObject;
        internal string _name;
        internal string _id;

        protected Func<int> _function;
        protected DateTime _executionDate;
        protected DateInterval _executionInterval;



        public Manager ParentManager {
            get { return _parentManager; } 
            set {
                if (value == null) 
                    throw new ArgumentNullException("Manager can't be null");

                if (_parentManager != null)
                    throw new ArgumentException("Manager already set");

                _parentManager = value;
            }
        }

        public object ReferenceToJobObject {
            get { return _referenceToJobObject; } 
            protected set {
                if (value == null) 
                    throw new ArgumentNullException("Manager can't be null");

                _referenceToJobObject = value;
            }
        }

        protected Func<int> Function {
            get { return _function;  } 
            set { 
                if (value == null) 
                    throw new ArgumentNullException("Manager can't be null");

                _function = value; 
            }
        }

        public string Name {
            get { return _name; } 
            protected set { _name = value; }
        }

        public string Id {
            get { return _id; } 
            protected set { _id = value; }
        }

        protected DateTime ExecutionDate {
            get { return _executionDate;  } 
            set { _executionDate = value; }
        }

        protected DateInterval ExecutionInterval {
            get { return _executionInterval;  } 
            set { _executionInterval = value; }
        }





        internal Job() {
        
        }

        internal Job(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval) {
            this.Id = Id;
            this.ReferenceToJobObject = Obj;
            this.ExecutionDate = ExecutionDate;
            this.ExecutionInterval = Interval;

            FindFunction();
        }


        public abstract bool IsRepeatable();


        public void Execute() {
            try {
                if (Function() == 0)
                    throw new BadReturnedValueException( "Function returned 0.");
            } catch(BadReturnedValueException brve) {
                throw; 
            } catch(Exception e) {
                throw new ExecutionException("Error while running function.", e);
            }
        }








        private void FindFunction() {
            Type ObjectType = ReferenceToJobObject.GetType();
            MethodInfo JobMethod = ObjectType.GetMethod("JMExecute");

            if (!JobMethod.ReturnType.Equals(typeof(int))) {
                throw new InvalidObjectException( 
                        "Type " + ObjectType.Name 
                                + " does not contains 'JMExecute' function.");
            }

            Function = 
                Expression.Lambda<Func<int>>(
                    Expression.Call(
                        Expression.Constant(ReferenceToJobObject), JobMethod
                    )
                ).Compile();
        }

    
    }




    public sealed class UniqueJob : Job {

        public UniqueJob(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval)
            : base(Id, Obj, ExecutionDate, Interval) {

        }

        public override bool IsRepeatable() {
            return false; 
        }
    
    }


    public sealed class RepeatableJob : Job {
        
        public RepeatableJob(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval)
            : base(Id, Obj, ExecutionDate, Interval) {

        }
    
        public override bool IsRepeatable() {
            return true; 
        } 

    }


}
