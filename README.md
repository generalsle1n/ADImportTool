# ADImportTool


This tool provides the functionality to synchronize user data from multiple connectors to an single ldap destination.
 


[![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://GitHub.com/Naereen/StrapDown.js/graphs/commit-activity)
[![BuildTestAndPublish](https://github.com/generalsle1n/ADImportTool/actions/workflows/main.yml/badge.svg)](https://github.com/generalsle1n/ADImportTool/actions/workflows/main.yml)

## Authors

- [@generalSle1n](https://github.com/generalsle1n)


## Acknowledgements

 - [Loggin Framework: Serilog](https://github.com/serilog/serilog)


## Tech Stack

**Framework:** ![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white) 
![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/github%20actions-%232671E5.svg?style=for-the-badge&logo=githubactions&logoColor=white)


**Server:** ![Linux](https://img.shields.io/badge/Linux-FCC624?style=for-the-badge&logo=linux&logoColor=black)
![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)


## Demo 

![](https://raw.githubusercontent.com/generalsle1n/ADImportTool/main/blob/demo1.gif)


## Run Locally

Clone the project

```bash
  git clone https://github.com/generalsle1n/ADImportTool/
```

Go to the project directory

```bash
  cd ADImportTool\ADImportTool
```

Install dependencies

```bash
  dotnet restore
```

Start the server

```bash
  dotnet run
```


## Roadmap

- Multiple LDAP Destinations
- 1 Attribute to multiple Ldap Attribute with check

## Lessons Learned

Used Reflection to make the code more reusable and simpler
## Installation

Publish the project with the following settings
- Config:               ```Release```
- TargetFramework:      ```.Net 6```
- Deployment:           ```SelfContained```
- Singlefile:           ```true```
- ReadyToRun:           ```true```
- Remove not used Code: ```true```

```bash
  sc.exe create "ADImportTool" binpath="C:\Path\To\ADImportTool.exe"
```
    
## Features
This tools adds on the first run all Connectors and then validate them if there ok.
Before the flow with the users start. The Program calculate which Connector is processed, this is getting done with the priority in the config.
Then each connector is reading the users, but before the users are getting added to the user store, the rule engine process each user and if needed to generate/modify user attributes.
When the rule engine is finsihed the user are getting added to the store.
After that the tool gets all User that match, with the in the config defined ldap filter, then it looks if some attributes need to be updated.
When the ldap user is not equal to the source it gets updated
All actions are logged to Console and in an file 

## Configuration
To run this project, you will need to add the following config variables to your ConnectorConfig.json file.
There is an example config in the repo
### Connectors



`Type`: The full Name of the Interfaceimpementation class **string**

`Name`: Name to determine the Connector **string**

`Attributes`: **Specified in a lower section**

`Priority`: Enter an Priority, the Connector with the highest priority get proccess last **int**

`DataSource`: Path to the users source **string**

`JsonArrayStart`: Enter the start Array Name of the source **string**

`ADPrimaryKey`: The AD Attribute where you link the source user and the ldap user **string**

`ADClearProperty`: Dont need to be specified, all attributes are getting cleared. Multiple Attributes are seperated by an semicolon **string**

`ManagerSettings`: **Specified in a lower section**

### Attributes

#### Settings

`Field`: Enter the name of the field which gets inserted into user

`Manipulation`: Enter the rule for the engine: Possible Values
- `RemoveLeading=Count`: Removes the first characters from an string, `Count` needs to be replaced with an **int**
- `GetFirstLeading=Count`:  Get the first characters from an string, `Count` needs to be replaced with an **int**

``GenerateFrom``: Enter the rule for the engine: Possible Values

- `{Field1}, {Field2} Data`: With this you can generate data from existing fields, there are no limitation into the amount of fields (In the example Field1, Field2) The upper example search for string with an start and end bracket and then try to resolve the brackets wit data: The rule would be generate the following string: "DataFromField1, DataFromField2 Data"  **string**

`Destination`: Enter the ldap attribute where the data should be stored **string**

`Type`: The full Name of the  class with the correct object **string**

#### Fields

All the fields need an json object after the upper settings decleration

- Active
- Mail
- FirstName
- LastName
- FullName
- Manager
- UnresolvedManager
- Position
- CostCenter
- EmployeeNumber
- TimecardNumber
- Salution
- Area
- Department
- Team

### ManagerSettings

#### Settings
`Enabled`: Specifiy if the Manager should be tried to get resolved **bool**

`ResolveManagerBy`: Enter the field from which attribute the manager should get resolved, the possible values are the fields from "Attributes" **string**

`Type`:The full Name of the  class with the correct object, there is currently only "ADImportTool.Model.ManagerSettings" available **string**

### Global Settings

`DomainName`: Enter the Name of the AD/Ldap Server **string**

`Port`: Enter the ldap port **int**

`UserName`: Enter the user name (sAMAccountname) **string**

`Passwortd`: Enter the password for the user **string**

`SearchBase`: Enter the ldap searchbase, where the programm should operate

`DefaultFilter`: Enter an valid Ldap Searchstring, only the user that match that, get synced **string**

`RefreshTime`: Enter the time in minutes how long it waits betwenn an cylce **int**

## Feedback

If you have any feedback, please open an issue or an pr

