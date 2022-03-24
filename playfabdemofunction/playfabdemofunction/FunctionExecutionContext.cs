using PlayFab.ProfilesModels;

namespace playfabdemofunction
{
	public class FunctionExecutionContext<T>
    {
        public EntityProfileBody CallerEntityProfile { get; set; }
        public TitleAuthenticationContext TitleAuthenticationContext { get; set; }
        public bool? GeneratePlayStreamEvent { get; set; }
        public T FunctionArgument { get; set; }
    }
}
