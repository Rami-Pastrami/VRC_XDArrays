//XDArrays - V1.1 Release - Multidimensional arrays for VRC made easier!
//Created by Rami-Pastrami
//Feel free to use in free/paid projects, but please credit!
//uncomment the "#define VRC_DEBUG" line to output messages in logs to aid debugging (this should
//be turned off for public releases to avoid log spam!)
//#define XDARRAYS_DEBUG
//uncomment any lines if you want to allow conversion.
//NOTE: Each conversion requires the class of that type!


using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class XDArray_Ints : UdonSharpBehaviour
{
    
	
	
	
	
	private void Start()
    {
        Debug.Log("XDIArrays is being utilized! Created by Rami-Pastrami! If you are reading this you are a nerd!");
#if (XDARRAYS_DEBUG)
        Debug.Log("XDIArrays Debug Mode is Enabled! If you are seeing this in a public map, please tell the creator to disable this!");
#endif

    }
	
    //////////////////////////////////////////////////////////////
    ////////////////XDArray Creation & Conversion/////////////////
    //////////////////////////////////////////////////////////////
    #region XDArray Creation & Conversion

    /// <summary>
    /// Converts normal arrays to XDIArrays.
    /// for dimensions, do [(length dimension 1, length dimension 2 , ... , length dimension n)] 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public int[] ConvertToXDIArr(int[] input, int[] dimensions)
    {
        int numDims = dimensions.Length;
        int startIndex = numDims + 1;
        int[] output = new int[(input.Length + numDims + 1)];

        output[0] = numDims; //number dimensions header

        for (int i = 1; i < startIndex; ++i) //dimension size header
        {
            output[i] = dimensions[(i - 1)];
        }

        for (int i = startIndex; i < output.Length; ++i)
        {
            output[i] = input[(i - startIndex)];
        }

#if XDARRAYS_DEBUG
        Debug.Log("Converted normal intArr to XDIArr with Dimensions {" + D_IArray2StringCSV(dimensions) + "}");
#endif

        return output;
    }

    /// <summary>
    /// Helper function for above, converts a 1D ints array to a 1D XDIArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public int[] ConvertTo1DIArr(int[] input)
    {
        int[] dimensions = new int[] { input.Length };
        return ConvertToXDIArr(input, dimensions);
    }

    /// <summary>
    /// Converts XDIArray into a normal int array by stripping header.
    /// WARNING: ALL Dimension information will be lost!
    /// </summary>
    /// <param name="XDIArr"></param>
    /// <returns></returns>
    public int[] ConvertToIArr(int[] XDIArr)
    {
        int offset = 1 + (XDIArr[0]);

#if XDARRAYS_DEBUG
        Debug.Log("Attempting to convert XDIArr to a normal IArr, which had " + (offset - 1).ToString() + " dimensions");
#endif

        int[] output = new int[(XDIArr.Length - offset)];
        for (int i = offset; i < XDIArr.Length; ++i)
        {
            output[(i - offset)] = XDIArr[i];
        }
        return output;
    }

    /// <summary>
    /// Create a zero'd XDIArray of given dimensions
    /// </summary>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public int[] CreateXDIArr(int[] dimensions)
    {
#if XDARRAYS_DEBUG
        Debug.Log("Creating empty XDIArray of dimensions {" + D_IArray2StringCSV(dimensions) + "}");
#endif
        int[] output = new int[(1 + dimensions.Length + MultiplyIntArrElements(dimensions))];
        output[0] = dimensions.Length;
        for (int i = 0; i < (dimensions.Length); ++i)
        {
            output[(i + 1)] = dimensions[i];
        }
        return output;
    }

    #endregion
	
    //////////////////////////////////////////////////////////////
    ////////////////////Indexing & Coordinates////////////////////
    //////////////////////////////////////////////////////////////
    #region Indexing & Coordinates

    /// <summary>
    /// returns dimension information from XDIArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public int[] ReadXDIArrDims(int[] input)
    {

#if XDARRAYS_DEBUG
        Debug.Log("Checking number of dimensions in input XDIArray...");
#endif

        int[] output = new int[(input[0])];
        for (int i = 0; i < (output.Length); ++i)
        {
            output[i] = (input[(i + 1)]);
        }

#if XDARRAYS_DEBUG
        Debug.Log("Confirmed dimensions are {" + D_IArray2StringCSV(output) + "}");
#endif

        return output;
    }

    /// <summary>
    /// Gets the raw index of the array from a given coordinate
    /// 
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
    public int ReadSingleElementFromXDIArr(int[] readFrom, int[] coords)
    {
#if XDARRAYS_DEBUG
        Debug.Log("Reading element in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDIArrDims(readFrom)) + "}");
#endif
        int[] kick = CalcPerCoordKick(ReadXDIArrDims(readFrom));
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
    public int ReadSingleElementFromXDIArr_OPT(int[] readFrom, int[] coords, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXD{L}}ArrDims(readFrom); 
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        return readFrom[GetRawIndexFromCoordsFull(coords, kick, indexOffset)];
    }

    /// <summary>
    /// read array segment of XDIArray into another XD{L}}Array of the same number of dimensions
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="readingStartCoords"></param>
    /// <param name="distFromCords"></param>
    /// <returns></returns>
    public int[] ReadXDIArrFromXDIArr(int[] readFrom, int[] readingStartCoords, int[] distFromCords)
    {
        int[] readingDims = ReadXDIArrDims(readFrom); //input num dims = output num dims
        int[] readTo = CreateXDIArr(distFromCords); //XDArray that the read data will be written to and outputted!
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
    public int[] WriteSingleElementToXDIArray(int[] writeTo, int[] coords, int ValueToWrite)
    {
#if XDARRAYS_DEBUG
        Debug.Log("writing value {" + ValueToWrite.ToString() + "} in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDIArrDims(writeTo)) + "}");
#endif        
        int[] kick = CalcPerCoordKick(ReadXDIArrDims(writeTo));
        int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// Loop-Optimized version of WriteSingleElementToXDIArray, follow instructions of what variables to pass.
    /// This is done to avoid repetitive calculations
    /// </summary>
    /// <param name="writeTo"></param>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    public int[] WriteSingleElementToXDIArray_OPT(int[] writeTo, int[] coords, int ValueToWrite, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXDIArrDims(readFrom); //for below!
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// overwrites set values of target XDIArray with source XDIArray starting from targetStartCoords
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetStartCoords"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public int[] WriteXDIArrToXDIArr(int[] target, int[] targetStartCoords, int[] source)
    {
        int[] targetDims = ReadXDIArrDims(target); //input num dims = output num dims
        int[] sourceDims = ReadXDIArrDims(source); //these dimensions should be smaller than the target (or the same size, but why bother using this then?)
        //sourceDims = size coords to write!
        int[] targetKick = CalcPerCoordKick(targetDims); //for calculating raw indexes from target coordinates
        int[] sourceKick = CalcPerCoordKick(sourceDims); //for calculating raw indexes from source coordinates
        int numElementsToWrite = MultiplyIntArrElements(sourceDims); //number of elements that will be written
        int indexOffsetFromHeader = targetDims.Length + 1; //This is the same for all XDIArrays involved here!

        int[] incrementingCoords = new int[targetDims.Length]; // starts at 0,0...n
        int rawIndexStartWritingTo = GetRawIndexFromCoordsFull(targetStartCoords, targetKick, indexOffsetFromHeader); //raw index starting point for writing
        int rawIndexStartReadingFrom = indexOffsetFromHeader; //GetRawIndexFromCoordsFull(incrementingCoords, sourceKick, indexOffsetFromHeader);

#if XDARRAYS_DEBUG
        Debug.Log("Writing to XDIArray of dimensions {" + D_IArray2StringCSV(targetDims) + "} starting from {" + D_IArray2StringCSV(targetStartCoords) + "} section of size {" + D_IArray2StringCSV(sourceDims) + "}");
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
    /// Appends a new dimension of length 1 to an XDIArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public int[] DimensionAddToXDIArray(int[] input)
    {
        int[] output = new int[input.Length + 1];
        output[0] = ((input[0]) + 1);


        //new dimensions
        for (int i = 1; i < (output[0]); ++i)
        {
            output[i] = input[i];
        }
        output[((input[0]) + 1)] = 1;


        //rest of data
        for (int i = (output[0]) + 1 ; i < output.Length; ++i)
        {
            output[i] = input[(i - 1)];
        }

        return output;
    }

    /// <summary>
    /// Removes a dimension from an XDI array.
    /// WARNING: DIMENSION MUST HAVE A LENGTH OF 1
    /// Dimension index starts from 0 (to remove second dimensions, dimToRemove should be 1)
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dimToRemove"></param>
    /// <returns></returns>
    public int[] DimensionsFlattenFromXDIArray(int[] input, int dimToRemove)
    {
#if XDARRAYS_DEBUG
        if((input[dimToRemove + 1])  != 1 )
        {
            Debug.Log("XDIARRAY WARNING: Attempting to flatten a dimension not the length of 1! This WILL cause problems!");
        }

#endif

        int[] output = new int[input.Length - 1];
        output[0] = ((input[0]) - 1);

        //new dimensions
        int index = 1;
        for (int i = 0; i < (input[0]); ++i)
        {
            Debug.Log(i.ToString());
            if(i != dimToRemove)
            {
                output[index] = input[(i + 1)];
                index++;
            }
        }

        //rest of data
        for(int i = (input[0]); i < output.Length; ++i)
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
	
    #endregion
	
    //////////////////////////////////////////////////////////////
    /////////////////////////Debug Related////////////////////////
    //////////////////////////////////////////////////////////////
    #region Debug Related


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