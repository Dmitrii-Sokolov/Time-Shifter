using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Tracker : MonoBehaviour
{
    [Flags]
    private enum ErrorType
    {
        NoError = 0,
        InvalidPath = 1,
        InvalidTime = 2,
        RequirePermission = 4
    }

    private readonly TimeSpan mCheckerShift = new TimeSpan(0, 0, 42);

    private TimeSpan mCurrentShift = TimeSpan.Zero;
    private TimeSpan mTotalShift = TimeSpan.Zero;
    private ErrorType mCurrentErrorState = ErrorType.NoError;
    private FileSystemWatcher mFileWatcher = new FileSystemWatcher();
    private Action mActionQueue = default;

    [SerializeField]
    private InputField mPath = default;

    [SerializeField]
    private InputField mYear = default;
    [SerializeField]
    private InputField mMonth = default;
    [SerializeField]
    private InputField mDay = default;
    [SerializeField]
    private InputField mHour = default;
    [SerializeField]
    private InputField mMinute = default;

    [SerializeField]
    private Text mTimeShift = default;
    [SerializeField]
    private Text mErrorMessage = default;

    private ErrorType CurrentErrorState
    {
        get { return mCurrentErrorState; }
        set
        {
            if (mCurrentErrorState != value)
            {
                mCurrentErrorState = value;
                CheckErrorMessage();
            }
        }
    }

    public void ResetTime()
    {
        AddTime(-mTotalShift);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void CheckPath()
    {
        OnPathChanged(mPath.text);
    }

    private void OnTimeChanged(string _)
    {
        if (
            int.TryParse(mYear.text, out var year) &&
            int.TryParse(mMonth.text, out var month) &&
            int.TryParse(mDay.text, out var day) &&
            int.TryParse(mHour.text, out var hour) &&
            int.TryParse(mMinute.text, out var minute))
        {
            mCurrentShift = new TimeSpan(365 * year + 30 * month + day, hour, minute, 0);
            CurrentErrorState = CurrentErrorState & ~ErrorType.InvalidTime;
        }
        else
        {
            mCurrentShift = TimeSpan.Zero;
            CurrentErrorState = CurrentErrorState | ErrorType.InvalidTime;
        }
    }

    private void OnPathChanged(string path)
    {
        if (File.Exists(path))
        {
            mFileWatcher.Path = Path.GetDirectoryName(path);
            mFileWatcher.Filter = Path.GetFileName(path);
            mFileWatcher.EnableRaisingEvents = true;
            CurrentErrorState = CurrentErrorState & ~ErrorType.InvalidPath;
        }
        else
        {
            mFileWatcher.EnableRaisingEvents = false;
            CurrentErrorState = CurrentErrorState | ErrorType.InvalidPath;
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        mActionQueue += () => AddTime(mCurrentShift);
    }

    private void AddTime(TimeSpan timeSpan)
    {
        mTotalShift += timeSpan;
        mTimeShift.text = mTotalShift.ToString(@"dd\.hh\.mm");

        TimeControl.GetSystemTime(out var time);
        time += timeSpan;
        TimeControl.SetSystemTime(in time);
    }

    private void CheckErrorMessage()
    {
        var message = new StringBuilder();
        var noErrors = mCurrentErrorState == ErrorType.NoError;

        if (noErrors)
        {
            mErrorMessage.color = Color.green;
            message.AppendSubline("OK");
        }
        else
        {
            mErrorMessage.color = Color.red;

            if ((CurrentErrorState & ErrorType.InvalidPath) != 0)
                message.AppendSubline("Invalid path");

            if ((CurrentErrorState & ErrorType.InvalidTime) != 0)
                message.AppendSubline("Invalid time");

            if ((CurrentErrorState & ErrorType.RequirePermission) != 0)
                message.AppendSubline("No time change permission");
        }

        mErrorMessage.text = message.ToString();
    }

    private void CheckPermissions()
    {
        TimeControl.GetSystemTime(out var originalTime);
        var shiftedTime = originalTime + mCheckerShift;
        TimeControl.SetSystemTime(in shiftedTime);
        TimeControl.GetSystemTime(out var changedTime);
        TimeControl.SetSystemTime(in originalTime);

        CurrentErrorState = changedTime == shiftedTime
            ? CurrentErrorState & ~ErrorType.RequirePermission
            : CurrentErrorState | ErrorType.RequirePermission;
    }

    private void Start()
    {
        Application.quitting += ResetTime;
        AddTime(mCurrentShift);
        CheckPermissions();
        CheckErrorMessage();

        mPath.onValueChanged.AddListener(OnPathChanged);
        mYear.onValueChanged.AddListener(OnTimeChanged);
        mMonth.onValueChanged.AddListener(OnTimeChanged);
        mDay.onValueChanged.AddListener(OnTimeChanged);
        mHour.onValueChanged.AddListener(OnTimeChanged);
        mMinute.onValueChanged.AddListener(OnTimeChanged);

        mFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
        mFileWatcher.Changed += OnFileChanged;
        mFileWatcher.EnableRaisingEvents = true;

        OnPathChanged(mPath.text);
        OnTimeChanged(string.Empty);
    }

    private void Update()
    {
        mActionQueue?.Invoke();
        mActionQueue = null;
    }

    private void OnDestroy()
    {
        mPath.onValueChanged.RemoveAllListeners();
        mYear.onValueChanged.RemoveAllListeners();
        mMonth.onValueChanged.RemoveAllListeners();
        mDay.onValueChanged.RemoveAllListeners();
        mHour.onValueChanged.RemoveAllListeners();
        mMinute.onValueChanged.RemoveAllListeners();
    }
}
