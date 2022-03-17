//XDArrays - V1.1 Release - Multidimensional arrays for VRC made easier!
//Created by Rami-Pastrami
//Feel free to use in free/paid projects, but please credit!
//uncomment the "#define VRC_DEBUG" line to output messages in logs to aid debugging (this should
//be turned off for public releases to avoid log spam!)
//#define XDARRAYS_DEBUG
//uncomment any lines if you want to allow conversion.
//NOTE: Each conversion requires the class of that type!
#define Convert_Color
#define Convert_Float
#define Convert_Int
#define Convert_Quaternion
#define Convert_Vector2
#define Convert_Vector3
#define Convert_Vector4

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class XDArray_Floats : UdonSharpBehaviour
{
 //Enable conversions by uncommenting above defines
#if (Convert_Color)
    XDArray_Colors XDC;
#endif
#if (Convert_Float)
	public XDArray_Floats XDF;
#endif
#if (Convert_Int)
	public XDArray_Ints XDI;
#endif
#if (Convert_Quaternion)
	public XDArray_Quaternions XDQ;
#endif
#if (Convert_Vector2)
	public XDArray_Vector2s XDV2;
#endif
#if (Convert_Vector3)
	public XDArray_Vector3s XDV3;
#endif
#if (Convert_Vector4)
	public XDArray_Vector4s XDV4;
#endif

	private void Start()
    {
        Debug.Log("XDFArrays is being utilized! Created by Rami-Pastrami! If you are reading this you are a nerd!");
		#if (XDARRAYS_DEBUG)
			Debug.Log("XDFArrays Debug Mode is Enabled! If you are seeing this in a public map, please tell the creator to disable this!");
		#endif

    }
	
    //////////////////////////////////////////////////////////////
    ////////////////XDArray Creation & Conversion/////////////////
    //////////////////////////////////////////////////////////////
    #region XDArray Creation & Conversion

    /// <summary>
    /// Converts normal arrays to XDFArrays.
    /// for dimensions, do [(length dimension 1, length dimension 2 , ... , length dimension n)] 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public float[] ConvertToXDFArr(float[] input, int[] dimensions)
    {
        int numDims = dimensions.Length;
        int startIndex = numDims + 1;
        float[] output = new float[(input.Length + numDims + 1)];

        output[0] = (float)numDims; //number dimensions header

        for (int i = 1; i < startIndex; ++i) //dimension size header
        {
            output[i] = (float)dimensions[(i - 1)];
        }

        for (int i = startIndex; i < output.Length; ++i)
        {
            output[i] = input[(i - startIndex)];
        }

		#if XDARRAYS_DEBUG
			Debug.Log("Converted normal floatArr to XDFArr with Dimensions {" + D_IArray2StringCSV(dimensions) + "}");
		#endif

        return output;
    }

    /// <summary>
    /// Helper function for above, converts a 1D floats array to a 1D XDFArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public float[] ConvertTo1DFArr(float[] input)
    {
        int[] dimensions = new int[] { input.Length };
        return ConvertToXDFArr(input, dimensions);
    }

    /// <summary>
    /// Converts XDFArray into a normal float array by stripping header.
    /// WARNING: ALL Dimension information will be lost!
    /// </summary>
    /// <param name="XDFArr"></param>
    /// <returns></returns>
    public float[] ConvertToFArr(float[] XDFArr)
    {
        int offset = 1 + Mathf.RoundToInt(XDFArr[0]);

		#if XDARRAYS_DEBUG
			Debug.Log("Attempting to convert XDFArr to a normal FArr, which had " + (offset - 1).ToString() + " dimensions");
		#endif

        float[] output = new float[(XDFArr.Length - offset)];
        for (int i = offset; i < XDFArr.Length; ++i)
        {
            output[(i - offset)] = XDFArr[i];
        }
        return output;
    }

    /// <summary>
    /// Create a zero'd XDFArray of given dimensions
    /// </summary>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public float[] CreateXDFArr(int[] dimensions)
    {
		#if XDARRAYS_DEBUG
			Debug.Log("Creating empty XDFArray of dimensions {" + D_IArray2StringCSV(dimensions) + "}");
		#endif
        float[] output = new float[(1 + dimensions.Length + MultiplyIntArrElements(dimensions))];
        output[0] = (float)dimensions.Length;
        for (int i = 0; i < (dimensions.Length); ++i)
        {
            output[(i + 1)] = (float)dimensions[i];
        }
        return output;
    }

    #endregion
	
    //////////////////////////////////////////////////////////////
    ////////////////////Indexing & Coordinates////////////////////
    //////////////////////////////////////////////////////////////
    #region Indexing & Coordinates

    /// <summary>
    /// returns dimension information from XDFArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public int[] ReadXDFArrDims(float[] input)
    {

		#if XDARRAYS_DEBUG
			Debug.Log("Checking number of dimensions in input XDFArray...");
		#endif

        int[] output = new int[Mathf.RoundToInt(input[0])];
        for (int i = 0; i < (output.Length); ++i)
        {
            output[i] = Mathf.RoundToInt(input[(i + 1)]);
        }
		#if XDARRAYS_DEBUG
			Debug.Log("Confirmed dimensions are {" + D_IArray2StringCSV(output) + "}");
		#endif
        return output;
    }

    /// <summary>
    /// Gets the raw index of the array from a given coordinate
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    private int GetRawIndexFromCoordsFull(int[] coords, int[] kick, int indexOffset)
    {
        for (int coordIndex = 0; coordIndex < kick.Length; ++coordIndex)
        {
            indexOffset += (coords[coordIndex] * kick[coordIndex]);
        }
        return indexOffset;
    }

    #endregion
	
    //////////////////////////////////////////////////////////////
    /////////////////////////Reading Data/////////////////////////
    //////////////////////////////////////////////////////////////
    #region Reading Data

    /// <summary>
    /// reads a single value of given coordinate from XDArray.
    /// This is a pretty heavy function so use the optimized version if running in massive loops
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="coords"></param>
    /// <returns></returns>
    public float ReadSingleElementFromXDFArr(float[] readFrom, int[] coords)
    {
		#if XDARRAYS_DEBUG
			Debug.Log("Reading element in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDFArrDims(readFrom)) + "}");
		#endif
        int[] kick = CalcPerCoordKick(ReadXDFArrDims(readFrom));
        int indexOffset = 1 + coords.Length;
        return readFrom[GetRawIndexFromCoordsFull(coords, kick, indexOffset)];
    }

    /// <summary>
    /// Loop-Optimized version of ReadSingleElementFromXD{L}}Arr, follow instructions of what variables to pass.
    /// This is done to avoid repetitive calculations
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    public float ReadSingleElementFromXDFArr_OPT(float[] readFrom, int[] coords, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXD{L}}ArrDims(readFrom); 
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        return readFrom[GetRawIndexFromCoordsFull(coords, kick, indexOffset)];
    }

    /// <summary>
    /// read array segment of XDFArray into another XD{L}}Array of the same number of dimensions
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="readingStartCoords"></param>
    /// <param name="distFromCords"></param>
    /// <returns></returns>
    public float[] ReadXDFArrFromXDFArr(float[] readFrom, int[] readingStartCoords, int[] distFromCords)
    {
        int[] readingDims = ReadXDFArrDims(readFrom); //input num dims = output num dims
        float[] readTo = CreateXDFArr(distFromCords); //XDArray that the read data will be written to and outputted!
        // readToDims is the same as distFromCords

        int[] readingKick = CalcPerCoordKick(readingDims); //for calculating raw indexes from source coordinates
        int[] readToKick = CalcPerCoordKick(distFromCords);

        int numElementsToRead = MultiplyIntArrElements(distFromCords);
        int indexOffsetFromHeader = readingDims.Length + 1; //This is the same for all XDFArrays involved here!

        int[] incrementingCoords = new int[readingDims.Length]; //starts at 0,0,...nDims
        int rawIndex_readFrom = GetRawIndexFromCoordsFull(readingStartCoords, readingKick, indexOffsetFromHeader);
        int rawIndex_readTo = indexOffsetFromHeader;

		#if XDARRAYS_DEBUG
			Debug.Log("Reading from XDArray of dimensions {" + D_IArray2StringCSV(readingDims) + "} starting from {" + D_IArray2StringCSV(readingStartCoords) + "} section of size {" + D_IArray2StringCSV(distFromCords) + "}");
		#endif

        readTo[rawIndex_readTo] = readFrom[rawIndex_readFrom]; //IncrementDimCoords is not smart enough to start with 0,0,0..., so we are doing that seperately
        for (int i = 1; i < numElementsToRead; ++i)
        {
            incrementingCoords = IncrementDimCoords(incrementingCoords, distFromCords, i);
			
			#if XDARRAYS_DEBUG
				Debug.Log("Reading Coordinate {" + D_IArray2StringCSV(AddIntArrs(readingStartCoords, incrementingCoords)) + "}");
			#endif
			
            rawIndex_readFrom = GetRawIndexFromCoordsFull(AddIntArrs(readingStartCoords, incrementingCoords), readingKick, indexOffsetFromHeader);
            rawIndex_readTo = GetRawIndexFromCoordsFull(incrementingCoords, readToKick, indexOffsetFromHeader);
			
			#if XDARRAYS_DEBUG
				Debug.Log("Wrote value {" + readFrom[rawIndex_readFrom] + "}");
			#endif
			
            readTo[rawIndex_readTo] = readFrom[rawIndex_readFrom];
        }

        return readTo;
    }

#endregion

    //////////////////////////////////////////////////////////////
    /////////////////////////Writing Data/////////////////////////
    //////////////////////////////////////////////////////////////
    #region Writing Data

    /// <summary>
    /// writes a single value of given coordinate to XDArray.
    /// This is a pretty heavy function so use the optimized version if running in massive loops 
    /// </summary>
    /// <param name="writeTo"></param>
    /// <param name="coords"></param>
    /// <param name="ValueToWrite"></param>
    /// <returns></returns>
    public float[] WriteSingleElementToXDFArray(float[] writeTo, int[] coords, float ValueToWrite)
    {
		#if XDARRAYS_DEBUG
			Debug.Log("writing value {" + ValueToWrite.ToString() + "} in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDFArrDims(writeTo)) + "}");
		#endif        
        int[] kick = CalcPerCoordKick(ReadXDFArrDims(writeTo));
        int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// Loop-Optimized version of WriteSingleElementToXDFArray, follow instructions of what variables to pass.
    /// This is done to avoid repetitive calculations
    /// </summary>
    /// <param name="writeTo"></param>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    public float[] WriteSingleElementToXDFArray_OPT(float[] writeTo, int[] coords, float ValueToWrite, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXDFArrDims(readFrom); //for below!
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// overwrites set values of target XDFArray with source XDFArray starting from targetStartCoords
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetStartCoords"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public float[] WriteXDFArrToXDFArr(float[] target, int[] targetStartCoords, float[] source)
    {
        int[] targetDims = ReadXDFArrDims(target); //input num dims = output num dims
        int[] sourceDims = ReadXDFArrDims(source); //these dimensions should be smaller than the target (or the same size, but why bother using this then?)
        //sourceDims = size coords to write!
        int[] targetKick = CalcPerCoordKick(targetDims); //for calculating raw indexes from target coordinates
        int[] sourceKick = CalcPerCoordKick(sourceDims); //for calculating raw indexes from source coordinates
        int numElementsToWrite = MultiplyIntArrElements(sourceDims); //number of elements that will be written
        int indexOffsetFromHeader = targetDims.Length + 1; //This is the same for all XDFArrays involved here!

        int[] incrementingCoords = new int[targetDims.Length]; // starts at 0,0...n
        int rawIndexStartWritingTo = GetRawIndexFromCoordsFull(targetStartCoords, targetKick, indexOffsetFromHeader); //raw index starting point for writing
        int rawIndexStartReadingFrom = indexOffsetFromHeader; //GetRawIndexFromCoordsFull(incrementingCoords, sourceKick, indexOffsetFromHeader);

		#if XDARRAYS_DEBUG
			Debug.Log("Writing to XDFArray of dimensions {" + D_IArray2StringCSV(targetDims) + "} starting from {" + D_IArray2StringCSV(targetStartCoords) + "} section of size {" + D_IArray2StringCSV(sourceDims) + "}");
		#endif

        target[rawIndexStartWritingTo] = source[rawIndexStartReadingFrom];
        for (int i = 1; i < numElementsToWrite; ++i)
        {
            incrementingCoords = IncrementDimCoords(incrementingCoords, sourceDims, i);
            rawIndexStartWritingTo = GetRawIndexFromCoordsFull(AddIntArrs(targetStartCoords, incrementingCoords), targetKick, indexOffsetFromHeader);
            rawIndexStartReadingFrom = GetRawIndexFromCoordsFull(incrementingCoords, sourceKick, indexOffsetFromHeader);
			
			#if XDARRAYS_DEBUG
				Debug.Log("Writing {" + source[rawIndexStartReadingFrom].ToString() + "} to coordinate {" + D_IArray2StringCSV(AddIntArrs(targetStartCoords, incrementingCoords)) + "}");
			#endif

            target[rawIndexStartWritingTo] = source[rawIndexStartReadingFrom];
        }

        return target;
    }

    #endregion
	
    //////////////////////////////////////////////////////////////
    ///////////////////Manipulating Dimensions////////////////////
    //////////////////////////////////////////////////////////////	
	#region Manipulating Dimensions

    /// <summary>
    /// Appends a new dimension of length 1 to an XDFArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public float[] DimensionAddToXDFArray(float[] input)
    {
        float[] output = new float[input.Length + 1];
        output[0] = (float)((input[0]) + 1);

        //new dimensions
        for (int i = 1; i < Mathf.RoundToInt(output[0]); ++i)
        {
            output[i] = input[i];
        }
        output[Mathf.RoundToInt((input[0]) + 1)] = 1;

        //rest of data
        for (int i = Mathf.RoundToInt(output[0]) + 1 ; i < output.Length; ++i)
        {
            output[i] = input[(i - 1)];
        }

        return output;
    }

    /// <summary>
    /// Removes a dimension from an XDF array.
    /// WARNING: DIMENSION MUST HAVE A LENGTH OF 1
    /// Dimension index starts from 0 (to remove second dimensions, dimToRemove should be 1)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dimToRemove"></param>
    /// <returns></returns>
    public float[] DimensionsFlattenFromXDFArray(float[] input, int dimToRemove)
    {
		#if XDARRAYS_DEBUG
			if(Mathf.RoundToInt(input[dimToRemove + 1])  != 1 )
			{
				Debug.Log("XDFARRAY WARNING: Attempting to flatten a dimension not the length of 1! This WILL cause problems!");
			}
		#endif

        float[] output = new float[input.Length - 1];
        output[0] = ((float)(input[0]) - 1);

        //new dimensions
        int index = 1;
        for (int i = 0; i < Mathf.RoundToInt(input[0]); ++i)
        {
            Debug.Log(i.ToString());
            if(i != dimToRemove)
            {
                output[index] = input[(i + 1)];
                index++;
            }
        }

        //rest of data
        for(int i = Mathf.RoundToInt(input[0]); i < output.Length; ++i)
        {
            output[i] = input[(i + 1)];
        }

        return output;
    }
	
	#endregion
	
	//////////////////////////////////////////////////////////////
    ////////////////////////////Casting///////////////////////////
    //////////////////////////////////////////////////////////////
    #region Casting
	

    /// <summary>
    /// Converts XDArray of Colors to XDArray of Floats.
    /// Note: Color elements will be added along a 4 long additional dimension
    /// </summary>
    /// <param name="XDCInput"></param>
    /// <param name="dimsOfInput"></param>
    /// <returns></returns>
	public float[] XDCToXDF(Color[] XDCInput, int[] dimsOfInput)
	{
		int[] newDims = AddDimOfLength(dimsOfInput, 4);
		float[] output = CreateXDFArr(newDims);
        int[] incrementingCoordOut = new int[newDims.Length];
        int[] incrementingCoordIn = new int[dimsOfInput.Length];
        int[] kickOut = CalcPerCoordKick(newDims);
        int[] kickIn = CalcPerCoordKick(dimsOfInput);
        int indexOffsetOut = 1 + newDims.Length;
        int indexOffsetIn = newDims.Length;
        int numElementsIn = MultiplyIntArrElements(dimsOfInput);


        //manually insert first set of values to output
        output[(indexOffsetOut)] = XDCInput[indexOffsetIn].r; //for each element in the V3 array
        output[(indexOffsetOut + (1 * kickOut[(kickOut.Length - 1)]))] = XDCInput[indexOffsetIn].g;
        output[(indexOffsetOut + (2 * kickOut[(kickOut.Length - 1)]))] = XDCInput[indexOffsetIn].b;
		output[(indexOffsetOut + (3 * kickOut[(kickOut.Length - 1)]))] = XDCInput[indexOffsetIn].a;

        //we done the first coordinate, skip it
        incrementingCoordOut[0] = 1;
        incrementingCoordIn[0] = 1;

        //now loop through all other coordinates
        for(int i = 1; i < numElementsIn; ++i)
        {
            incrementingCoordOut = IncrementDimCoords(incrementingCoordOut, newDims, i);
            incrementingCoordIn = IncrementDimCoords(incrementingCoordIn, dimsOfInput, i);
            incrementingCoordOut[incrementingCoordIn.Length] = 0; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDC.ReadSingleElementFromXDCArr_OPT(XDCInput, incrementingCoordIn, kickIn, indexOffsetIn).r, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 1; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDC.ReadSingleElementFromXDCArr_OPT(XDCInput, incrementingCoordIn, kickIn, indexOffsetIn).g, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 2; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDC.ReadSingleElementFromXDCArr_OPT(XDCInput, incrementingCoordIn, kickIn, indexOffsetIn).b, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 3; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDC.ReadSingleElementFromXDCArr_OPT(XDCInput, incrementingCoordIn, kickIn, indexOffsetIn).a, kickOut, indexOffsetOut);
        }
        return output;
    }
	/// <summary>
    /// Converts XDArray of Ints to XDArray of Floats.
    /// </summary>
    /// <param name="XDIInput"></param>
    /// <returns></returns>
	public float[] XDIToXDF(int[] XDIInput)
	{
		float[] output = new float[XDIInput.Length];
		for(int i = 0; i < XDIInput.Length; ++i)
		{
			output[i] = (float)XDIInput[i];
		}
		return output;
	}
	
    /// <summary>
    /// Converts XDArray of Strings to XDArray of Floats.
    /// </summary>
    /// <param name="XDSInput"></param>
    /// <returns></returns>
	public float[] XDSToXDF(string[] XDSInput)
	{
		float[] output = new float[XDSInput.Length];
		for(int i = 0; i < XDSInput.Length; ++i)
		{
			output[i] = float.Parse(XDSInput[i]);
		}
		return output;
	}
	
    /// <summary>
    /// Converts XDArray of Quaternion to XDArray of Floats.
    /// Note: Quaternion elements will be added along a 4 long additional dimension
    /// </summary>
    /// <param name="XDQInput"></param>
    /// <param name="dimsOfInput"></param>
    /// <returns></returns>
	public float[] XDQToXDF(Quaternion[] XDQInput, int[] dimsOfInput)
	{
		int[] newDims = AddDimOfLength(dimsOfInput, 4);
		float[] output = CreateXDFArr(newDims);
        int[] incrementingCoordOut = new int[newDims.Length];
        int[] incrementingCoordIn = new int[dimsOfInput.Length];
        int[] kickOut = CalcPerCoordKick(newDims);
        int[] kickIn = CalcPerCoordKick(dimsOfInput);
        int indexOffsetOut = 1 + newDims.Length;
        int indexOffsetIn = newDims.Length;
        int numElementsIn = MultiplyIntArrElements(dimsOfInput);


        //manually insert first set of values to output
        output[(indexOffsetOut)] = XDQInput[indexOffsetIn].x; //for each element in the V3 array
        output[(indexOffsetOut + (1 * kickOut[(kickOut.Length - 1)]))] = XDQInput[indexOffsetIn].y;
        output[(indexOffsetOut + (2 * kickOut[(kickOut.Length - 1)]))] = XDQInput[indexOffsetIn].z;
		output[(indexOffsetOut + (3 * kickOut[(kickOut.Length - 1)]))] = XDQInput[indexOffsetIn].w;

        //we done the first coordinate, skip it
        incrementingCoordOut[0] = 1;
        incrementingCoordIn[0] = 1;

        //now loop through all other coordinates
        for(int i = 1; i < numElementsIn; ++i)
        {
            incrementingCoordOut = IncrementDimCoords(incrementingCoordOut, newDims, i);
            incrementingCoordIn = IncrementDimCoords(incrementingCoordIn, dimsOfInput, i);
            incrementingCoordOut[incrementingCoordIn.Length] = 0; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDQ.ReadSingleElementFromXDQArr_OPT(XDQInput, incrementingCoordIn, kickIn, indexOffsetIn).x, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 1; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDQ.ReadSingleElementFromXDQArr_OPT(XDQInput, incrementingCoordIn, kickIn, indexOffsetIn).y, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 2; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDQ.ReadSingleElementFromXDQArr_OPT(XDQInput, incrementingCoordIn, kickIn, indexOffsetIn).z, kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 3; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDQ.ReadSingleElementFromXDQArr_OPT(XDQInput, incrementingCoordIn, kickIn, indexOffsetIn).w, kickOut, indexOffsetOut);
        }
        return output;
    }
	
    /// <summary>
    /// Converts XDArray of Vector2 to XDArray of Floats.
    /// Note: Vector2 elements will be added along a 2 long additional dimension
    /// </summary>
    /// <param name="XDV2Input"></param>
    /// <param name="dimsOfInput"></param>
    /// <returns></returns>
	public float[] XDV2ToXDF(Vector2[] XDV2Input, int[] dimsOfInput)
	{
		int[] newDims = AddDimOfLength(dimsOfInput, 2);
		float[] output = CreateXDFArr(newDims);
        int[] incrementingCoordOut = new int[newDims.Length];
        int[] incrementingCoordIn = new int[dimsOfInput.Length];
        int[] kickOut = CalcPerCoordKick(newDims);
        int[] kickIn = CalcPerCoordKick(dimsOfInput);
        int indexOffsetOut = 1 + newDims.Length;
        int indexOffsetIn = newDims.Length;
        int numElementsIn = MultiplyIntArrElements(dimsOfInput);


        //manually insert first set of values to output
        output[(indexOffsetOut)] = XDV2Input[indexOffsetIn][0]; //for each element in the V2 array
        output[(indexOffsetOut + (1 * kickOut[(kickOut.Length - 1)]))] = XDV2Input[indexOffsetIn][1];


        //we done the first coordinate, skip it
        incrementingCoordOut[0] = 1;
        incrementingCoordIn[0] = 1;

        //now loop through all other coordinates
        for(int i = 1; i < numElementsIn; ++i)
        {
            incrementingCoordOut = IncrementDimCoords(incrementingCoordOut, newDims, i);
            incrementingCoordIn = IncrementDimCoords(incrementingCoordIn, dimsOfInput, i);
            incrementingCoordOut[incrementingCoordIn.Length] = 0; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV2.ReadSingleElementFromXDV2Arr_OPT(XDV2Input, incrementingCoordIn, kickIn, indexOffsetIn)[0], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 1; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV2.ReadSingleElementFromXDV2Arr_OPT(XDV2Input, incrementingCoordIn, kickIn, indexOffsetIn)[1], kickOut, indexOffsetOut);
        }

        return output;
    }
	
    /// <summary>
    /// Converts XDArray of Vector3 to XDArray of Floats.
    /// Note: Vector3 elements will be added along a 3 long additional dimension
    /// </summary>
    /// <param name="XDV3Input"></param>
    /// <param name="dimsOfInput"></param>
    /// <returns></returns>
	public float[] XDV3ToXDF(Vector3[] XDV3Input, int[] dimsOfInput)
	{
		int[] newDims = AddDimOfLength(dimsOfInput, 3);
		float[] output = CreateXDFArr(newDims);
        int[] incrementingCoordOut = new int[newDims.Length];
        int[] incrementingCoordIn = new int[dimsOfInput.Length];
        int[] kickOut = CalcPerCoordKick(newDims);
        int[] kickIn = CalcPerCoordKick(dimsOfInput);
        int indexOffsetOut = 1 + newDims.Length;
        int indexOffsetIn = newDims.Length;
        int numElementsIn = MultiplyIntArrElements(dimsOfInput);


        //manually insert first set of values to output
        output[(indexOffsetOut)] = XDV3Input[indexOffsetIn][0]; //for each element in the V3 array
        output[(indexOffsetOut + (1 * kickOut[(kickOut.Length - 1)]))] = XDV3Input[indexOffsetIn][1];
        output[(indexOffsetOut + (2 * kickOut[(kickOut.Length - 1)]))] = XDV3Input[indexOffsetIn][2];

        //we done the first coordinate, skip it
        incrementingCoordOut[0] = 1;
        incrementingCoordIn[0] = 1;

        //now loop through all other coordinates
        for(int i = 1; i < numElementsIn; ++i)
        {
            incrementingCoordOut = IncrementDimCoords(incrementingCoordOut, newDims, i);
            incrementingCoordIn = IncrementDimCoords(incrementingCoordIn, dimsOfInput, i);
            incrementingCoordOut[incrementingCoordIn.Length] = 0; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV3.ReadSingleElementFromXDV3Arr_OPT(XDV3Input, incrementingCoordIn, kickIn, indexOffsetIn)[0], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 1; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV3.ReadSingleElementFromXDV3Arr_OPT(XDV3Input, incrementingCoordIn, kickIn, indexOffsetIn)[1], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 2; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV3.ReadSingleElementFromXDV3Arr_OPT(XDV3Input, incrementingCoordIn, kickIn, indexOffsetIn)[2], kickOut, indexOffsetOut);

        }

        return output;
    }
	
    /// <summary>
    /// Converts XDArray of Vector4 to XDArray of Floats.
    /// Note: Vector4 elements will be added along a 4 long additional dimension
    /// </summary>
    /// <param name="XDV4Input"></param>
    /// <param name="dimsOfInput"></param>
    /// <returns></returns>
	public float[] XDV4ToXDF(Vector4[] XDV4Input, int[] dimsOfInput)
	{
		int[] newDims = AddDimOfLength(dimsOfInput, 4);
		float[] output = CreateXDFArr(newDims);
        int[] incrementingCoordOut = new int[newDims.Length];
        int[] incrementingCoordIn = new int[dimsOfInput.Length];
        int[] kickOut = CalcPerCoordKick(newDims);
        int[] kickIn = CalcPerCoordKick(dimsOfInput);
        int indexOffsetOut = 1 + newDims.Length;
        int indexOffsetIn = newDims.Length;
        int numElementsIn = MultiplyIntArrElements(dimsOfInput);


        //manually insert first set of values to output
        output[(indexOffsetOut)] = XDV4Input[indexOffsetIn][0]; //for each element in the V3 array
        output[(indexOffsetOut + (1 * kickOut[(kickOut.Length - 1)]))] = XDV4Input[indexOffsetIn][1];
        output[(indexOffsetOut + (2 * kickOut[(kickOut.Length - 1)]))] = XDV4Input[indexOffsetIn][2];
		output[(indexOffsetOut + (3 * kickOut[(kickOut.Length - 1)]))] = XDV4Input[indexOffsetIn][3];

        //we done the first coordinate, skip it
        incrementingCoordOut[0] = 1;
        incrementingCoordIn[0] = 1;

        //now loop through all other coordinates
        for(int i = 1; i < numElementsIn; ++i)
        {
            incrementingCoordOut = IncrementDimCoords(incrementingCoordOut, newDims, i);
            incrementingCoordIn = IncrementDimCoords(incrementingCoordIn, dimsOfInput, i);
            incrementingCoordOut[incrementingCoordIn.Length] = 0; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV4.ReadSingleElementFromXDV4Arr_OPT(XDV4Input, incrementingCoordIn, kickIn, indexOffsetIn)[0], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 1; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV4.ReadSingleElementFromXDV4Arr_OPT(XDV4Input, incrementingCoordIn, kickIn, indexOffsetIn)[1], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 2; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV4.ReadSingleElementFromXDV4Arr_OPT(XDV4Input, incrementingCoordIn, kickIn, indexOffsetIn)[2], kickOut, indexOffsetOut);
            incrementingCoordOut[incrementingCoordIn.Length] = 3; //manual increment
            WriteSingleElementToXDFArray_OPT(output, incrementingCoordOut, XDV4.ReadSingleElementFromXDV4Arr_OPT(XDV4Input, incrementingCoordIn, kickIn, indexOffsetIn)[3], kickOut, indexOffsetOut);
        }
        return output;
    }
	    #endregion
	
    //////////////////////////////////////////////////////////////
    /////////////////////////Debug Related////////////////////////
    //////////////////////////////////////////////////////////////
    #region Debug Related
	/// <summary>
    /// Converts a 1DFArr into a single string CSV
    /// </summary>
    /// <param name="i1DFArr"></param>
    /// <returns></returns>
    public string D_1DFArray2StringCSV(float[] i1DFArr)
    {
        string output = i1DFArr[2].ToString();
        for (int i = 3; i < i1DFArr.Length; ++i)
        {
            output = output + ", " + i1DFArr[i].ToString();
        }
        return output.ToString();
    }
	/// <summary>
    /// Converts a Int_Array (Not an XDArray!) into a single string CSV
    /// </summary>
    /// <param name="iArr"></param>
    /// <returns></returns>
    private string D_IArray2StringCSV(int[] iArr)
    {
        string output = iArr[0].ToString();
        for (int i = 1; i < iArr.Length; ++i)
        {
            output = output + ", " + iArr[i].ToString();
        }
        //return (output + iArr[(iArr.Length - 1)].ToString());
        return output.ToString();
    }

    #endregion
	
    //////////////////////////////////////////////////////////////
    ////////////////////////Work Functions////////////////////////
    //////////////////////////////////////////////////////////////
    #region Work Functions
	/// <summary>
    /// Simply adds the elements of 2 int Arrs
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    private int[] AddIntArrs(int[] A, int[] B)
    {
        int[] C = new int[A.Length]; //arrays are ref types
        for (int i = 0; i < A.Length; ++i)
        {
            C[i] = A[i] + B[i];
        }
        return C;
    }

    /// <summary>
    /// Multiplies the elements of an int array together into one output
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private int MultiplyIntArrElements(int[] input)
    {
        int output = 1;
        for (int i = 0; i < input.Length; ++i)
        {
            output *= input[i];
        }
        return output;
    }

    /// <summary>
    /// Calculates a kick (multiplication based offset) for coordinates
    /// </summary>
    /// <param name="dims"></param>
    /// <returns></returns>
    private int[] CalcPerCoordKick(int[] dims)
    {
        int[] output = new int[dims.Length];
        output[0] = 1;
        for (int i = 1; i < dims.Length; ++i)
        {
            output[i] = output[(i - 1)] * dims[(i - 1)];
        }
        return output;
    }

    /// <summary>
    /// Increments dim coords in accordance to output index
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="dimMaxSizes"></param>
    /// <param name="ind"></param>
    /// <returns></returns>
    private int[] IncrementDimCoords(int[] coord, int[] dimMaxSizes, int ind)
    {
        coord[0] = ind % dimMaxSizes[0]; //Modulo loop
        if (coord[0] == 0) //increment a later layer
        {
            int curLayer = 1;
            while (true)
            {
                ++coord[curLayer]; //increment layer
                if (coord[curLayer] != dimMaxSizes[curLayer])
                {
                    //we are good, break out
                    break;
                }
                //else, the current layer has gone too far, set to 0 and increment the next one instead
                coord[curLayer] = 0;
                ++curLayer;
            }
        }
        return coord;
    }
	
	/// <summary>
    /// Appends additional dimension to dimension Array of specified length
    /// </summary>
    /// <param name="dimArr"></param>
    /// <param name="newDimLength"></param>
    /// <returns></returns>
	private int[] AddDimOfLength(int[] dimArr, int newDimLength)
	{
		int[] output = new int[dimArr.Length + 1];
		for(int i = 0; i < dimArr.Length; ++i)
		{
			output[i] = dimArr[i];
		}
		output[dimArr.Length] = newDimLength;
		return output;
	}
    #endregion
}