namespace ArDrone2.Client.Commands
{
    public class ExitBootstrapModeCommand : SetConfigurationCommand
    {
        public ExitBootstrapModeCommand()
            : base("general:navdata_demo", "TRUE")
        { }
    }
}