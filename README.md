Simple sample showing how to use the DotNetOpenAuth library to get access to VitaDock cloud data.
Beware that this version of the sample uses a special compiled version of the DotNetOpenAuth library that allows for signing messages with timestamps compatible with VitaDock.

Feel free to use this sample it if you can. You have to 

1) Have an authorized and valid account with VitaDock and

2) register an application with VitaDock

3) insert a valid Application Token and secret in the web.config file and

4) make sure that the registered application is allowed to read and write temperature data and 

5) also register a callback to http://localhost:9201/VitaDock.aspx (or whatever you want to change this to).

The application connects with vitacloud.medisanaspace.com. (Set in VitaDockConsumer.cs) This was built with Visual Studio 2012 targeting the .Net Framework version 4.5 on windows 8. But I believe it should easily work in other configurations as well.