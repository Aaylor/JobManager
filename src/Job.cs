using JobManager;
using JobManager.Date;
using JobManager.Exceptions;

using Newtonsoft.Json;

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JobManager {
   

    /// <summary>
    /// The job emulation.
    /// 
    ///
    /// Example to create a job.
    /// <code>
    ///
    /// public class TestJob {
    ///     public int JMExecute() {
    ///         Console.WriteLine("Hello World.");
    ///         return 1;
    ///     }
    ///
    ///
    ///     public static void Main(string args[]) {
    ///
    ///         DateTime dt = DateTime.Now;
    ///
    ///         DateInterval di = new DateInterval();        
    ///         di.Minutes = 20;
    ///
    ///         Job j = new RepeatableJob("RepJob1", new TestJob(), dt, di);
    ///
    ///     }
    /// }
    ///
    ///
    /// </code>
    /// </summary>
    public abstract class Job {

        internal Manager _parentManager;
        internal object _referenceToJobObject;
        internal string _name;
        internal string _id;
        internal Func<int> _function;
        internal DateTime _executionDate;
        internal DateInterval _executionInterval;




        /// <summary>
        /// The parent manager. Used to have a link with the parent manager.
        /// </summary>
        [JsonIgnore]
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


        /// <summary>
        /// Reference to the object created for the job.
        /// </summary>
        [JsonIgnore]
        public object ReferenceToJobObject {
            get { return _referenceToJobObject; } 
            protected set {
                if (value == null) 
                    throw new ArgumentNullException("Manager can't be null");

                _referenceToJobObject = value;
            }
        }


        /// <summary>
        /// The function to execute when it's time to.
        /// Inside the object, it has to be named as "JMExecute"
        /// </summary>
        [JsonIgnore]
        public Func<int> Function {
            get { return _function;  } 
            set { 
                if (value == null) 
                    throw new ArgumentNullException("Manager can't be null");

                _function = value; 
            }
        }


        /// <summary>
        /// Name of the object created.
        /// It could be only "UniqueJob" and "RepeatableJob".
        /// </summary>
        public string Name {
            get { return _name; } 
            protected set { _name = value; }
        }



        /// <summary>
        /// Id given by the user.
        /// It's use to differenciate jobs inside the manager.
        /// </summary>
        public string Id {
            get { return _id; } 
            protected set { _id = value; }
        }



        /// <summary>
        /// The next execution date.
        /// It gives the date to when the job has to be execute.
        /// </summary>
        public DateTime ExecutionDate {
            get { return _executionDate;  } 
            set { _executionDate = value; }
        }



        /// <summary>
        /// The date interval.
        /// Time to wait between two executions.
        /// </summary>
        public DateInterval ExecutionInterval {
            get { return _executionInterval;  } 
            set { _executionInterval = value; }
        }



        /// <summary>
        /// Use to forbid inheritance.
        /// </summary>
        internal Job() {
        
        }


        /// <summary>
        /// Create the instance. Internal is use to forbid inheritance.
        /// Raise InvalidObjectException if the object doesn't have "JMExecute"
        /// function.
        /// </summary>
        internal Job(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval) {
            this.Id = Id;
            this.ReferenceToJobObject = Obj;
            this.ExecutionDate = ExecutionDate;
            this.ExecutionInterval = Interval;

            FindFunction();
        }


        /// <summary>
        /// Job repetetion.
        /// </summary>
        /// <returns>true if the job is repeatable.</returns>
        public abstract bool IsRepeatable();


        /// <summary>
        /// Function to execute when it's time.
        /// </summary>
        public void Execute() {
            try {
                if (Function() == 0)
                    throw new BadReturnedValueException("Function returned 0.");
            } catch(BadReturnedValueException) {
                throw; 
            } catch(Exception e) {
                throw new ExecutionException("Error while running function.", e);
            }
        }


        /// <summary>
        /// Add the interval to know the next date.
        /// </summary>
        public void ChangeToNextDate() {
            ExecutionDate = 
                ExecutionDate.AddYears(ExecutionInterval.Years)
                             .AddMonths(ExecutionInterval.Months)
                             .AddDays(ExecutionInterval.Days)
                             .AddHours(ExecutionInterval.Hours)
                             .AddMinutes(ExecutionInterval.Minutes)
                             .AddSeconds(ExecutionInterval.Seconds)
                             .AddMilliseconds(ExecutionInterval.Milliseconds);
        }



        /// <summary>
        /// Use to find "JMExecute" function inside the given object.
        /// Throw InvalidObjectException if it doesn't exists.
        /// </summary>
        private void FindFunction() {
            Type ObjectType = ReferenceToJobObject.GetType();
            MethodInfo JobMethod = ObjectType.GetMethod("JMExecute");

            if (!JobMethod.ReturnType.Equals(typeof(int))) {
                throw new InvalidObjectException( 
                        "Type " + ObjectType.Name 
                                + " doesn't contain 'JMExecute'" 
                                + " method returning an integer");
            }

            Function = 
                Expression.Lambda<Func<int>>(
                    Expression.Call(
                        Expression.Constant(ReferenceToJobObject), JobMethod
                    )
                ).Compile();
        }

    
    }



    /// <summary>
    /// Represent a unique job.
    /// It will be remove just after its execution.
    /// </summary>
    public sealed class UniqueJob : Job {

        /// <summary>
        /// Create an empty instance. Only used by the json loader.
        /// </summary>
        public UniqueJob() {

        }

        /// <summary>
        /// Create the instance with given data
        /// </summary>
        public UniqueJob(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval)
            : base(Id, Obj, ExecutionDate, Interval) {
            this.Name = "UniqueJob";
        }

        public override bool IsRepeatable() {
            return false; 
        }
    
    }


    /// <summary>
    /// Represent a repeatable job.
    /// </summary>
    public sealed class RepeatableJob : Job {

        /// <summary>
        /// Create an empty instance. Only used by the json loader.
        /// </summary>
        public RepeatableJob() {
        
        }
        
        /// <summary>
        /// Create the instance with given data.
        /// </summary>
        public RepeatableJob(string Id, object Obj, 
                DateTime ExecutionDate, DateInterval Interval)
            : base(Id, Obj, ExecutionDate, Interval) {
            this.Name = "RepeatableJob";
        }
    
        public override bool IsRepeatable() {
            return true; 
        } 

    }


}
