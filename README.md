[![Kit project](https://raw.githubusercontent.com/chubrik/Kit/master/logo-64x64.png)](#)
# Kit &nbsp;&middot;&nbsp; [![Build Status](https://travis-ci.org/chubrik/Kit.svg?branch=master)](https://travis-ci.org/chubrik/Kit)
The powerful engine for various C# projects. Has advanced diagnostics and the most useful features.

# How to use
```c#
void Main(string[] args) {
  Kit.Setup(...); // optional
  Kit.Execute(MyApp);
}
async Task MyApp(CancellationToken ct) {
  // sync or async method
}
```
