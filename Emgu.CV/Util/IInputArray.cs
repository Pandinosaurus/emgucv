﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;

namespace Emgu.CV
{
   /// <summary>
   /// This is the proxy class for passing read-only input arrays into OpenCV functions.
   /// </summary>
   public interface IInputArray
   {
      /// <summary>
      /// The unmanaged pointer to the input array.
      /// </summary>
      IntPtr InputArrayPtr { get; }
   }

   public partial class CvInvoke
   {
      /// <summary>
      /// Release the InputArray
      /// </summary>
      /// <param name="arr">Pointer to the input array</param>
      [DllImport(CvInvoke.EXTERN_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      public extern static void cveInputArrayRelease(ref IntPtr arr);
   }

   public class InputArray : UnmanagedObject, IInputArray
   {
      static InputArray()
      {
         CvInvoke.CheckLibraryLoaded();
      }

      private enum DataType
      {
         Scalar, 
         Double
      }
      private IntPtr _dataPtr;
      private DataType _dataType;
      
      public InputArray(MCvScalar scalar)
      {
         _dataPtr = cveScalarCreate(ref scalar);
         _dataType = DataType.Scalar;
         _ptr = cveInputArrayFromScalar(_dataPtr);
      }
      public InputArray(double scalar)
      {
         _dataPtr = Marshal.AllocHGlobal(sizeof(double));
         _dataType = DataType.Double;
         Marshal.Copy(new double[] { scalar }, 0, _dataPtr, 1);
         _ptr = cveInputArrayFromDouble(_dataPtr);
      }

      public static explicit operator InputArray(double scalar)
      {
         return new InputArray(scalar);
      }

      public static explicit operator InputArray(MCvScalar scalar)
      {
         return new InputArray(scalar);
      }

      protected override void DisposeObject()
      {
         if (_ptr != IntPtr.Zero)
            CvInvoke.cveInputArrayRelease(ref _ptr);

         if (_dataPtr != IntPtr.Zero)
         {
            if (_dataType == DataType.Scalar)
               cveScalarRelease(ref _dataPtr);
            else if (_dataType == DataType.Double)
            {
               Marshal.FreeHGlobal(_dataPtr);
               _dataPtr = IntPtr.Zero;
            }

            Debug.Assert(_dataPtr == IntPtr.Zero);
         }
      }

      public IntPtr InputArrayPtr
      {
         get { return _ptr; }
      }

      [DllImport(CvInvoke.EXTERN_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      internal extern static IntPtr cveScalarCreate(ref MCvScalar scalar);

      [DllImport(CvInvoke.EXTERN_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      internal extern static void cveScalarRelease(ref IntPtr scalar);

      [DllImport(CvInvoke.EXTERN_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      internal extern static IntPtr cveInputArrayFromScalar(IntPtr scalar);

      [DllImport(CvInvoke.EXTERN_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      internal extern static IntPtr cveInputArrayFromDouble(IntPtr scalar);

   }
}