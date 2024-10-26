namespace SmartTaskManagementAPI.System;

public class Result
{
    public bool flag { get; set; }
    public int code { get; set; }
    public string message { get; set; }
    public object data { get; set; }

    public Result()
    {
        
    }

    public Result(bool flag, int code, string message, object data)
    {
        this.flag = flag;
        this.code = code;
        this.message = message;
        this.data = data;
    }
    
    public Result(bool flag, int code, string message)
    {
        this.flag = flag;
        this.code = code;
        this.message = message;
        
    }
}