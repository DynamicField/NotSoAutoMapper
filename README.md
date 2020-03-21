# HandmadeMapper
HandmadeMapper is a simple and extensible library to map DTOs (or anything!) using bare, but powerful, expressions.

It also supports dependency injection.

[![Build Status](https://dev.azure.com/jeuxjeux20/HandmadeMapper/_apis/build/status/jeuxjeux20.HandmadeMapper?branchName=master)](https://dev.azure.com/jeuxjeux20/HandmadeMapper/_build/latest?definitionId=1&branchName=master) 

* NuGet packages :
  * **HandmadeMapper** 
  
    ![Nuget](https://img.shields.io/nuget/v/HandmadeMapper?style=plastic) 
  * **HandmadeMapper.Extensions.Ioc.Base** (Base library to use any IoC container)  
  
    ![Nuget](https://img.shields.io/nuget/v/HandmadeMapper.Extensions.Ioc.Base?style=plastic) 
  * **HandmadeMapper.Extensions.Ioc.DependencyInjection**   
    (Support for `Microsoft.Extensions.DependencyInjection`)
  
    ![Nuget](https://img.shields.io/nuget/v/HandmadeMapper.Extensions.Ioc.Base?style=plastic) 


* [Getting started](https://github.com/jeuxjeux20/HandmadeMapper/wiki/Getting-started)

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
    Cat = Mapper.Include(x.Cat, catDtoMapper) // Use the catDtoMapper
});

PersonDto personDto = personDtoMapper.Map(somePerson); // personDto.Cat is a CatDto!
Console.WriteLine($"{personDto.FirstName} has a cute cat named {personDto.Cat.Name}");
// >>> James has a cute cat named Felix
```
