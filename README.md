<h1 align="center">VRC XDArrays</h1>
<h3 align="center">Multidimensional Arrays for VrChat Udonsharp!</h3>


XDArrays is a set of classes with built in functions to emulate the use of multidimensional arrays within the confines of modern udonsharp 1 dimensional arrays. There is 1 class per ever supported variable type.

Code is fully commented and documented for ease of use during development! 

#Abilities
- Create Multidimensional Arrays of specified dimensions and size
- read From XDArrays, either single elements given their coordinates or a range of elements that will be outputted into another XDArray
- write To XDArrays, with the same methods as above
- convert normal 1D arrays to XDArrays and back
- debug mode that can be toggled on from within the source, exports useful information to log when testing

#Supported Types
- floats
- ints
- strings
- Vector2/3/4
- Quaternions

#quick guide

Import the unity package into your project. Udonsharp must be installed. If we want to make a float XDArray, reference the float XDArray file from your script at top after including it in the unity world on some object.

'public XDArray_Floats XDF;'

Lets create a new XDArray, in this case, a 3D 2 by 4 by 6 float array, using the above syntax:
'float[] exampleXDArray = XDF.CreateXDFArr(new int[] {2, 4, 6});'
In this case, this function "CreateXDFArr" takes in an int array defining the dimensions of the XDArray. Since there are 3 dimensions, we know this array is 3D (3 Dimensional)


we can even confirm the dimensions of the XDArray
'int[] dimensionsCheck = ReadXDFArrDims(exampleXDArray);'

in coordinate [0, 1, 3], lets write value 10f.
'exampleXDArray = XDF.WriteSingleElementToXDFArray(exampleXDArray, new int[] {0, 1, 3}, 10f);'
*notice how the coordinate system starts at 0*

We can repeat this several times for other coordinates to write various values within the XDArray!


now, lets read the second layer of this 3D array, assuming X is width, Y is height, and Z is depth, and store it into another XDArray:
'float[] exampleReadingXDArray' = XDF.ReadXDFArrFromXDFArr( exampleXDArray, new int[] {0 1 0}, new int[] {2 1 6});'
in this case, our first coordinate indicates where we start reading (in this case, from 0 X and Z wise, and 1 Y wise, the second layer), and we read along the entire length (doesnt start at 0! you cannot measure a 0 length of something) of the XZ plane, but only 1 deep Y wise).

#Methods

XDArrays are all 1D arrays in C# in memory, and are technically referred as such in source.
They are composed of 3 parts
- index 0 lists the number of dimensions in the XDArray
- index 1 -> (number of dimensions + 1) lists the length of each given dimensions
- The actual data

Using clever indexing functions, we can use just this header to take a set of coordinates (given in as int arrays) and output values.

#TODO

- add more types if possible
- add additional helper functions for common tasks
