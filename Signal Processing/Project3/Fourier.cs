﻿using System;

namespace SignalProcessing {
    /// <summary>
    /// Static methods used to perform Fourier Transforms and related functions
    /// </summary>
    class Fourier {
        /// <summary>
        /// Applies the Fast Fourier Transform algorithm to a Signal
        /// </summary>
        /// <param name="signal">The Signal to apply FFT to</param>
        /// <returns>The Fourier Transform of a Signal</returns>
        public static Signal FFT(Signal signal) {
            int N = signal.Length;
            if (N == 1)
                return signal;
            if ((N & (N - 1)) != 0)
                throw new ArgumentOutOfRangeException("signal length must be a power of 2");
            Signal evenArr = new Signal(N / 2);
            Signal oddArr = new Signal(N / 2);
            for (int i = 0; i < N / 2; i++) {
                evenArr[i] = signal[2 * i];
            }
            evenArr = FFT(evenArr);
            for (int i = 0; i < N / 2; i++) {
                oddArr[i] = signal[2 * i + 1];
            }
            oddArr = FFT(oddArr);
            Signal result = new Signal(N);
            for (int k = 0; k < N / 2; k++) {
                double w = -2.0 * k * Math.PI / N;
                ComplexNumber wk = new ComplexNumber(Math.Cos(w), Math.Sin(w));
                ComplexNumber even = evenArr[k];
                ComplexNumber odd = oddArr[k];
                result[k] = even + (wk * odd);
                result[k + N / 2] = even - (wk * odd);
            }
            return result;
        }
        /// <summary>
        /// Applies the Fast Fourier Transform in 2 dimensions to a Signal in 2 dimensions
        /// </summary>
        /// <param name="signal">The Signal2D to apply FFT2D to</param>
        /// <returns>The Fourier Transform of the Signal2D</returns>
        public static Signal2D FFT2D(Signal2D signal) {
            Signal2D result = new Signal2D(signal.Height, signal.Width);
            for (int i = 0; i < result.Height; i++)
                result[i] = new ComplexNumber[signal[i].Length];
            //rows
            for (int n = 0; n < signal.Height; n++) {
                result[n] = FFT(signal[n]);
            }
            //columns
            for (int i = 0; i < signal[0].Length; i++) {
                ComplexNumber[] col = new ComplexNumber[signal.Height];
                for (int j = 0; j < col.Length; j++) {
                    col[j] = result[j][i];
                }
                col = FFT(col);
                for (int j = 0; j < col.Length; j++) {
                    result[j][i] = col[j];
                }
            }
            return result;
        }
        /// <summary>
        /// Applies the Inverted Fast Fourier Transform algorithm in 2 dimensions to a Signal
        /// </summary>
        /// <param name="signal">The Signal2D to apply IFFT2D to</param>
        /// <returns>The Inverted Fourier Transform of the Signal2D</returns>
        public static Signal2D InverseFFT2D(Signal2D signal) {
            Signal2D result = signal.GetConjugate();
            result = FFT2D(result);
            for (int i = 0; i < signal.Height; i++) {
                for (int j = 0; j < signal.Width; j++) {
                    result[i][j] /= signal.Width * signal.Height;
                }
            }
            return result;
        }
        /// <summary>
        /// Applies the Inverse Fast Fourier Transform algorithm to a Signal
        /// </summary>
        /// <param name="signal">The Signal to apply IFFT to</param>
        /// <returns>The Inverted Fourier Transform of the Signal</returns>
        public static Signal InverseFFT(Signal signal) {
            Signal result = signal.GetConjugate();
            result = FFT(result);
            result /= signal.Length;
            return result;
        }
        /// <summary>
        /// Applies Cross Correlation between 2 Signals
        /// </summary>
        /// <param name="x">The first Signal</param>
        /// <param name="y">The second Signal</param>
        /// <returns>The Cross Correlation of the 2 Signals</returns>
        public static Signal CrossCorrelation(Signal x, Signal y) {
            Signal Y = y;
            Signal X = x;
            if (y.Length < x.Length)
                Y = y.PadWithZeros(x.Length);
            if (x.Length < y.Length)
                X = x.PadWithZeros(y.Length);
            int shiftAmt = 0;
            Signal shift = new Signal(Y.Length + shiftAmt);
            for (int i = 0; i < shift.Length; i++) {
                if (i < shiftAmt)
                    shift[i] = 0;
                else {
                    shift[i] = Y[i - shiftAmt];
                }
            }
            for (int i = 0; i < Y.Length; i++) {
                Y[i] = shift[i];
            }
            X = FFT(X);
            Y = FFT(Y).GetConjugate();
            Signal XY = new Signal(X.Length);
            for (int i = 0; i < X.Length; i++) {
                XY[i] = X[i] * Y[i];
            }
            return InverseFFT(XY);
        }
        /// <summary>
        /// Applies Cross Convolution between a Signal and a Filter. Used to apply Filters to a Signal
        /// </summary>
        /// <param name="signal">The Signal to Filter</param>
        /// <param name="filter">The Filter to be used</param>
        /// <returns>The Filtered Signal</returns>
        public static Signal CrossConvolution(Signal signal, Signal filter) {
            Signal filterFFT = filter.PadWithZeros(signal.Length);
            Signal signalFFT = FFT(signal);
            filterFFT = FFT(filterFFT);
            Signal result = new Signal(signal.Length);
            for (int i = 0; i < signal.Length; i++) {
                result[i] = signalFFT[i] * filterFFT[i];
            }
            return InverseFFT(result);
        }
        /// <summary>
        /// Calculates the cross correlation of 2 dimensional Signals
        /// </summary>
        /// <param name="signal">The response Signal</param>
        /// <param name="pulse">The pulse Signal</param>
        /// <returns>The cross correlation</returns>
        public static Signal2D CrossCorrelation2D(Signal2D signal, Signal2D pulse) {
            return InverseFFT2D(FFT2D(pulse).GetConjugate() * FFT2D(signal));
        }
    }
}
