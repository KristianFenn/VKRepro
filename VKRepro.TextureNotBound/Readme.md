Project to demonstrate an issue in the DesktopVK backend with unbound texture parameters in shaders.

There's two constants at the top of the game file - 
- `_enableNormalMaps` sets the shader to use a technique that includes optional normal mapping.
- `_provideNormalMap` sets whether the game will actually provide a normal map, and enable the flag to use normal mapping.