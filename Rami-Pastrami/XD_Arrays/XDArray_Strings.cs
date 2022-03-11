﻿//XDArrays - V0.8 Prerelease - Multidimensional arrays for VRC made easier!
//Created by Rami-Pastrami
//Feel free to use in free/paid projects, but please credit!
//uncomment the "#define VRC_DEBUG" line to output messages in logs to aid debugging (this should
//be turned off for public releases to avoid log spam!)
//#define XDARRAYS_DEBUG

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class XDArray_Strings : UdonSharpBehaviour
{
private void Start()
    {
        Debug.Log("XDSArrays is being utilized! Created by Rami-Pastrami! If you are reading this you are a nerd!");
#if (XDARRAYS_DEBUG)
        Debug.Log("XDSArrays Debug Mode is Enabled! If you are seeing this in a public map, please tell the creator to disable this!");
#endif

    }

    //////////////////////////////////////////////////////////////
    ////////////////XDArray Creation & Conversion/////////////////
    //////////////////////////////////////////////////////////////
    #region XDArray Creation & Conversion

    /// <summary>
    /// Converts normal arrays to XDSArrays.
    /// for dimensions, do [(length dimension 1, length dimension 2 , ... , length dimension n)] 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public string[] ConvertToXDSArr(string[] input, int[] dimensions)
    {
        int numDims = dimensions.Length;
        int startIndex = numDims + 1;
        string[] output = new string[(input.Length + numDims + 1)];

        output[0] = numDims.ToString(); ; //number dimensions header

        for (int i = 1; i < startIndex; ++i) //dimension size header
        {
            output[i] = dimensions[(i - 1)].ToString();
        }

        for (int i = startIndex; i < output.Length; ++i)
        {
            output[i] = input[(i - startIndex)];
        }

#if XDARRAYS_DEBUG
        Debug.Log("Converted normal stringArr to XDSArr with Dimensions {" + D_IArray2StringCSV(dimensions) + "}");
#endif

        return output;
    }

    /// <summary>
    /// Helper function for above, converts a 1D string array to a 1D XDSArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public string[] ConvertTo1DFArr(string[] input)
    {
        int[] dimensions = new int[] { input.Length };
        return ConvertToXDSArr(input, dimensions);
    }

    /// <summary>
    /// Converts XDSArray into a normal string array by stripping header.
    /// WARNING: ALL Dimension information will be lost!
    /// </summary>
    /// <param name="XDSArr"></param>
    /// <returns></returns>
    public string[] ConvertToFArr(string[] XDSArr)
    {
        int offset = 1 + int.Parse(XDSArr[0]);

#if XDARRAYS_DEBUG
        Debug.Log("Attempting to convert XDSArr to a normal FArr, which had " + (offset - 1).ToString() + " dimensions");
#endif

        string[] output = new string[(XDSArr.Length - offset)];
        for (int i = offset; i < XDSArr.Length; ++i)
        {
            output[(i - offset)] = XDSArr[i];
        }
        return output;
    }

    /// <summary>
    /// Create a zero'd XDSArray of given dimensions
    /// </summary>
    /// <param name="dimensions"></param>
    /// <returns></returns>
    public string[] CreateXDSArr(int[] dimensions)
    {
#if XDARRAYS_DEBUG
        Debug.Log("Creating empty XDArray of dimensions {" + D_IArray2StringCSV(dimensions) + "}");
#endif
        string[] output = new string[(1 + dimensions.Length + MultiplyIntArrElements(dimensions))];
        output[0] = dimensions.Length.ToString();
        for (int i = 0; i < (dimensions.Length); ++i)
        {
            output[(i + 1)] = dimensions[i].ToString();
        }
        return output;
    }

    #endregion

    //////////////////////////////////////////////////////////////
    //////////////Indexing, Coordinates, & Dimensions/////////////
    //////////////////////////////////////////////////////////////
    #region Indexing, Coordinates, & Dimensions

    /// <summary>
    /// returns dimension information from XDSArray
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public int[] ReadXDSArrDims(string[] input)
    {

#if XDARRAYS_DEBUG
        Debug.Log("Checking number of dimensions in input XDArray...");
#endif

        int[] output = new int[int.Parse(input[0])];
        for (int i = 0; i < (output.Length); ++i)
        {
            output[i] = int.Parse(input[(i + 1)]);
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
    public string ReadSingleElementFromXDSArr(string[] readFrom, int[] coords)
    {
#if XDARRAYS_DEBUG
        Debug.Log("Reading element in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDSArrDims(readFrom)) + "}");
#endif
        int[] kick = CalcPerCoordKick(ReadXDSArrDims(readFrom));
        int indexOffset = 1 + coords.Length;
        return readFrom[GetRawIndexFromCoordsFull(coords, kick, indexOffset)];
    }

    /// <summary>
    /// Loop-Optimized version of ReadSingleElementFromXDSArr, follow instructions of what variables to pass.
    /// This is done to avoid repetitive calculations
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    public string ReadSingleElementFromXDSArr_OPT(string[] readFrom, int[] coords, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXDSArrDims(readFrom); 
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        return readFrom[GetRawIndexFromCoordsFull(coords, kick, indexOffset)];
    }

    /// <summary>
    /// read array segment of XDSArray into another XDSArray of the same number of dimensions
    /// </summary>
    /// <param name="readFrom"></param>
    /// <param name="readingStartCoords"></param>
    /// <param name="distFromCords"></param>
    /// <returns></returns>
    public string[] ReadXDSArrFromXDSArr(string[] readFrom, int[] readingStartCoords, int[] distFromCords)
    {
        int[] readingDims = ReadXDSArrDims(readFrom); //input num dims = output num dims
        string[] readTo = CreateXDSArr(distFromCords); //XDArray that the read data will be written to and outputted!
        // readToDims is the same as distFromCords

        int[] readingKick = CalcPerCoordKick(readingDims); //for calculating raw indexes from source coordinates
        int[] readToKick = CalcPerCoordKick(distFromCords);

        int numElementsToRead = MultiplyIntArrElements(distFromCords);
        int indexOffsetFromHeader = readingDims.Length + 1; //This is the same for all XDSArrays involved here!

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
    public string[] WriteSingleElementToXDSArray(string[] writeTo, int[] coords, string ValueToWrite)
    {
#if XDARRAYS_DEBUG
        Debug.Log("writing value {" + ValueToWrite.ToString() + "} in coord {" + D_IArray2StringCSV(coords) + "} from XDArray of dimensions {" + D_IArray2StringCSV(ReadXDSArrDims(writeTo)) + "}");
#endif        
        int[] kick = CalcPerCoordKick(ReadXDSArrDims(writeTo));
        int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// Loop-Optimized version of WriteSingleElementToXDSArray, follow instructions of what variables to pass.
    /// This is done to avoid repetitive calculations
    /// </summary>
    /// <param name="writeTo"></param>
    /// <param name="coords"></param>
    /// <param name="kick"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    public string[] WriteSingleElementToXDSArray_OPT(string[] writeTo, int[] coords, string ValueToWrite, int[] kick, int indexOffset)
    {
        //calculate the following following this method. Not included in function for optimization reasons
        //but whatever is calling this function can get this functions inputs with the following methods
        //int[] dims = ReadXDSArrDims(readFrom); //for below!
        //int[] kick = CalcPerCoordKick(dims);
        //int indexOffset = 1 + coords.Length;
        writeTo[GetRawIndexFromCoordsFull(coords, kick, indexOffset)] = ValueToWrite;
        return writeTo;
    }

    /// <summary>
    /// overwrites set values of target XDRArray with source XDRArray starting from targetStartCoords
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetStartCoords"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public string[] WriteXDSArrToXDSArr(string[] target, int[] targetStartCoords, string[] source)
    {
        int[] targetDims = ReadXDSArrDims(target); //input num dims = output num dims
        int[] sourceDims = ReadXDSArrDims(source); //these dimensions should be smaller than the target (or the same size, but why bother using this then?)
        //sourceDims = size coords to write!
        int[] targetKick = CalcPerCoordKick(targetDims); //for calculating raw indexes from target coordinates
        int[] sourceKick = CalcPerCoordKick(sourceDims); //for calculating raw indexes from source coordinates
        int numElementsToWrite = MultiplyIntArrElements(sourceDims); //number of elements that will be written
        int indexOffsetFromHeader = targetDims.Length + 1; //This is the same for all XDSArrays involved here!

        int[] incrementingCoords = new int[targetDims.Length]; // starts at 0,0...n
        int rawIndexStartWritingTo = GetRawIndexFromCoordsFull(targetStartCoords, targetKick, indexOffsetFromHeader); //raw index starting point for writing
        int rawIndexStartReadingFrom = indexOffsetFromHeader; //GetRawIndexFromCoordsFull(incrementingCoords, sourceKick, indexOffsetFromHeader);

#if XDARRAYS_DEBUG
        Debug.Log("Writing to XDArray of dimensions {" + D_IArray2StringCSV(targetDims) + "} starting from {" + D_IArray2StringCSV(targetStartCoords) + "} section of size {" + D_IArray2StringCSV(sourceDims) + "}");
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
    /////////////////////////Debug Related////////////////////////
    //////////////////////////////////////////////////////////////
    #region Debug Related

    /// <summary>
    /// Converts a 1DFArr into a single string CSV
    /// </summary>
    /// <param name="i1DFArr"></param>
    /// <returns></returns>
    public string D_1DFArray2StringCSV(string[] i1DFArr)
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


    /*
    public string D_2DFArray2StringCSV(string[] i2DFArr)
    {
        int[] dims = ReadXDSArrDims(i2DFArr);
        int[] coordsToRead = new int[2];
        int[] endCoords = new int[] {}
        string[] cacheArr = ReadXDSArrFromXDSArr(i2DFArr, coordsToRead)


    }
    */
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

    #endregion
}