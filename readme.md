# Mouser/EPM Tech assessment

The documentation asks what I'd change and where and why; well there's a few things.

First of all, which I've implemented, was to include Swashbuckle for aspnetcore so that the controllers are easy to test and debug.

Second, I'd want some unit tests, which given the current structure of the project isn't really possible. The requirements (which are very clear, thanks for that) state the types to be used and those are already provided in the referenced DLL. I understand why but also, it's generally bad practise to expost internal types like that and for preference I'd have gone with DTOs and C# Record types. However, this would not fulfil the brief, so...

Next, there's logic in the controllers. Normally I wouldn't do this - in point of fact, normally I'd use [LanguageExt by Paul Louth](https://github.com/louthy/language-ext) so that I had nice Functional pipelines with elevated types everywhere and a set of extension methods for splitting those down to unsafe types at application boundaries, such as controllers or on input and output from the data storage mechanism, but again this would not fulfil the brief.

However, it does mean that each part of the pipeline is extremely easy to unit test. Given the requirement to use provided types and DLL, I can't really unit test at all; I'd be stuck with integration tests and although those have their place, that place isn't really development as such.

Normally, my controllers look a lot more like this - 

```csharp
    [Route("api/[controller]")]
    [ApiController]
    public class NugetController : ControllerBase
    {
        private readonly INugetService _nugetService;

        public NugetController(INugetService nugetService) => _nugetService = nugetService;


        [HttpGet("{term}")]
        public async Task<IActionResult> GetNugetResults(string term) =>
            await _nugetService.TryGetNugetPackagesAsync(term)
                .ToActionResult();
```

Which you can find in my [Reactive UI Compelling Example](https://github.com/richbryant/ReactiveUI.CompellingExample) if you're interested, along with a set of extension methods for making that nice `ToActionResult()` bit work.

My last real issue - apart from having to remember how to use MVC at all, it's been _ages_ - is with the bit about making product names unique. I've done with an extension method and unfortunately, the neatest way to do this is to make it recursive over the entire list of product names. While this works, it absolutely doesn't scale. It's fine for 100 and maybe even 200 products but get into the high 10,000s and the speed and amount of IOps would be untenable - put it this way, in a cloud solution it would cost you a fortune to run.

I couldn't see a way to avoid this while meeting the requirements and I don't have access to the datastore. As for why the whole list of products? Well basically because otherwise I'd be conflicting `screw` with `screw1` with `screwdriver`. I could have queried on `StartsWith(name)` but unless you can use `Span<T>` that just means you're asking LINQ to do string manipulation which is incredibly inefficient.

Again, LanguageExt would have allowed me to map a function using Span<T> over the top of the elevated type should that prove a better option.

This is the kind of thing that's done better with a UDF on the data side. If relational databases are good for anything, it's processing batch data in a single efficient operation and they are far better at doing so than middle tier code could ever be.

Regardless, I quite enjoyed it.
