# NotSoAutoMapper
A library to map your DTOs with reusable expressions.

[![Build Status](https://dev.azure.com/jeuxjeux20/NotSoAutoMapper/_apis/build/status/jeuxjeux20.NotSoAutoMapper?branchName=master)](https://dev.azure.com/jeuxjeux20/NotSoAutoMapper/_build/latest?definitionId=1&branchName=master) 

* NuGet packages :
  * **NotSoAutoMapper** 
  
    ![Nuget](https://img.shields.io/nuget/v/NotSoAutoMapper?style=plastic) 
  * **NotSoAutoMapper.Extensions.Ioc.Base** (Base library to use any IoC container)  
  
    ![Nuget](https://img.shields.io/nuget/v/NotSoAutoMapper.Extensions.Ioc.DependencyInjection?style=plastic) 
  * **NotSoAutoMapper.Extensions.Ioc.DependencyInjection**   
    (Support for `Microsoft.Extensions.DependencyInjection`)


* [Getting started](https://github.com/jeuxjeux20/NotSoAutoMapper/wiki/Getting-started)

## Showcase

```cs
var catDtoMapper = new Mapper<Cat,CatDto>(x => new CatDto
{
    Id = x.Id,
    Name = x.Name,
    CutenessLevel = x.CutenessLevel
});
var personDtoMapper = new Mapper<Person, PersonDto>(x => new PersonDto
{
    Id = x.Id,
    FirstName = x.FirstName,
    LastName = x.LastName,
    Cat = x.Cat.MapWith(catDtoMapper) // Use the catDtoMapper
});

PersonDto personDto = personDtoMapper.Map(somePerson); // personDto.Cat is a CatDto!
Console.WriteLine($"{personDto.FirstName} has a cute cat named {personDto.Cat.Name}");
// >>> James has a cute cat named Felix
```
