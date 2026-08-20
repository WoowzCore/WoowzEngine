namespace WEEO;

public class ExceptionWEE : Exception{
    public ExceptionWEE(                                           ) : base("Не указана ошибка!"    ){}
    public ExceptionWEE(string? Message                            ) : base(Message                 ){}
    public ExceptionWEE(string? Message, Exception? ParentException) : base(Message, ParentException){}
}