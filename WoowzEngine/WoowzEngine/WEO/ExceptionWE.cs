namespace WEO;

public class ExceptionWE : Exception{
    public ExceptionWE(                                           ) : base("Не указана ошибка!"    ){}
    public ExceptionWE(string? Message                            ) : base(Message                 ){}
    public ExceptionWE(string? Message, Exception? ParentException) : base(Message, ParentException){}
}