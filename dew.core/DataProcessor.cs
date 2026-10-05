public delegate void DataProcessor<TData, TFrom, TTo>(ref TData data, TFrom from, TTo to) where TData : struct;
public delegate void DataProcessor<T>(ref T data) where T : struct;
