# TypedExpando
A class that supports dynamic typed properties



##Add properties to the object on runtime
```c#
 var O = new TypedExpando();
O.AddProperty("Name", typeof(string));
O.AddProperty("Age", typeof(int));
```

##Now you can use that properties and make bindings to it
```c#
var D = (dynamic)O;
D.Name = "Rafa";
D.Age = 22;
```

##Property types will be checked at runtime
```c#
//runtime exception: 
D.Age = "Hello";
```

##Implements `INotifyPropertyChanged` and `IDictionary<string, object>` interfaces
```c#
//Property change notification supports binding:
D.PropertyChanged += (sender, e) => { ... }

//IDictionary for merging seamlessly with existing code:
D["Age"] = 25;
foreach(var KeyValuePair in D) { ... }

//Add a typed property named Id with type int
D.Add("Id", 1);
```

nuget: `Install-Package TypedExpando`
