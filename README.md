#JobManager


This is a little JobManager in c#.


### Library

[Newtonsoft Json](http://james.newtonking.com/json) for the json parsing.

### How to use

Example, you want to write Hello every minute:

```c#
using JobManager;
using JobManager.Date;
using System;

public class Hello {

	public int JMExecute() {
		Console.WriteLine("Hello World!");
		
		return 1;
	}

}

public class TestJob {

	public static void Main(String[] args) {
		
		// Job Creation
		DateTime dt = DateTime.Now;
		DateInterval di = new DateInterval();
		di.Minutes = 1;
		
		Job j = new RepeatableJob("HelloJob1", new Hello(), dt, di);
		
		
		
		// Manager Creation
		// Tick every 30 seconds
		Manager m = new Manager("path/to/json/save/file", "path/to/log/file", 30000);

		m.AddJob(j);		
	}

}
```

The manager automatically save as a json structure every information needed to a job.
It will reload a job if it already exists in the json file given.

Example with the json structure given by the code above :

```json
[
	{
		"Name": "RepeatableJob",
		"Id"  : "HelloJob1",
		"ExecutionDate" : ...insert current date here...
		"ExecutionInterval" : {
			"Years"   	   : 0,
			"Months"  	   : 0,
			"Days"    	   : 0,
			"Hours"   	   : 0,
			"Minutes" 	   : 1,
			"Seconds" 	   : 0,
			"Milliseconds" : 0
		}
	}
]
```
