using WEEO;

public static class Bootstrap{
    public static int Main(string[] Args){
        try{
            WEE.Main.Start(Args);
            return WEE.Main.Stop();
        }catch(Exception e){
            throw new ExceptionWEE("Произошла ошибка в Bootstrap!", e);
        }
    }
}