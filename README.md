# ScheduleCalculator is class which offers us methods to:
* Calculate the finish DateTime of a working task, based on start date and duration.
* Calaculate duration between two dates. 
the calculated duration and end dates takes into account only working times, excluding public holidays and off-peak hours.


# Setting a holidays 
```
SetHolidays(new Holiday[] {new(new(2023,01,01)"New year"),new(new(2023,5,1),"Labour Day")}
```

You can also set a day off that's spread over 2 days or more.

