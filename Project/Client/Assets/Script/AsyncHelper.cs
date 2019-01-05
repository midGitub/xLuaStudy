using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 按照指定方式执行异步操作的帮助类，
/// 若操作为同步操作，会导致递归执行，此时慎用
/// </summary>
public class AsyncHelper
{
    /// <summary>
    /// 按顺序执行指定的任务序列，完成所有任务后执行Final
    /// </summary>
    /// <param name="tasks">任务序列数组</param>
    /// <param name="Final">结束处理函数，参数为tasks中每个任务对应的回调code</param>
    public int Series(System.Action<System.Action<int>>[] tasks, System.Action<int[]> Final)
    {
        if (_tasks != null)
        {
            return (int)LocalCode.ASYNC_BUSY;
        }
        if (tasks.Length == 0)
        {
            return (int)LocalCode.NEED_TASK;
        }
        _tasks = new List<System.Action<int>>();
        _codes = new int[tasks.Length];
        int len = tasks.Length;

        System.Action<int> cb = (code) =>
        {
            _codes[len - _tasks.Count - 1] = code;
            if (_tasks.Count > 0)
            {
                _tasks[0](code);
            }
            else
            {
                _tasks = null;
                var tmp = _codes;
                _codes = null;
                if (Final != null)
                    Final(tmp);

            }
        };

        for (int i = 0; i < tasks.Length; i++)
        {
            var action = tasks[i];

            _tasks.Add((code) =>
            {
                _tasks.RemoveAt(0);
                action(cb);
            });
        }

        StartFirst();
        return (int)LocalCode.SUCCESS;
    }
    /// <summary>
    /// 按流水线方式执行指定的任务序列，当碰到第一个传入非0回调code的任务时，跳过余下的任务，执行Final
    /// </summary>
    /// <param name="tasks">任务序列数组</param>
    /// <param name="Final">结束处理函数，参数为最后一个执行的任务对应的回调code</param>
    public int Waterfall(System.Action<System.Action<int>>[] tasks, System.Action<int> Final)
    {
        if (_tasks != null)
        {
            return (int)LocalCode.ASYNC_BUSY;
        }
        if (tasks.Length == 0)
        {
            return (int)LocalCode.NEED_TASK;
        }
        _tasks = new List<System.Action<int>>();

        System.Action<int> cb = (code) =>
        {
            if (_tasks.Count > 0 && code == (int)LocalCode.SUCCESS)
            {
                _tasks[0](code);
            }
            else
            {
                _tasks = null;
                if (Final != null)
                    Final(code);
            }
        };

        for (int i = 0; i < tasks.Length; i++)
        {
            var action = tasks[i];

            _tasks.Add((code) =>
            {
                _tasks.RemoveAt(0);
                action(cb);
            });
        }

        StartFirst();
        return (int)LocalCode.SUCCESS;
    }
    /// <summary>
    /// 并行执行指定的任务序列，完成所有任务后执行Final
    /// </summary>
    /// <param name="tasks">任务序列数组</param>
    /// <param name="Final">结束处理函数，参数为tasks中每个任务对应的回调code</param>
    public int Parallel(System.Action<System.Action<int>>[] tasks, System.Action<int[]> Final)
    {
        if (_tasks != null)
        {
            return (int)LocalCode.ASYNC_BUSY;
        }
        if (tasks.Length == 0)
        {
            return (int)LocalCode.NEED_TASK;
        }
        _codes = new int[tasks.Length];

        _completed = 0;

        for (int i = 0; i < tasks.Length; i++)
        {
            int idx = i;
            System.Action<int> cb = (code) =>
            {
                _completed++;
                _codes[idx] = code;
                if (_completed == _codes.Length)
                {
                    var tmp = _codes;
                    _codes = null;
                    if (Final != null)
                    {
                        Final(tmp);
                    }
                }
            };
            tasks[i](cb);
        }

        return (int)LocalCode.SUCCESS;
    }
    private void StartFirst()
    {
        _tasks[0](0);
    }

    private List<System.Action<int>> _tasks;
    private int[] _codes;
    private int _completed = 0;
}