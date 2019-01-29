using ProtoBuf.Meta;

namespace SimpleRemoteMethods.Bases
{
    public static class Statics
    {
        static Statics()
        {
            RuntimeTypeModel.Default.AutoAddMissingTypes = true;
            RuntimeTypeModel.Default.AutoCompile = true;
        }

        public static void Settings()
        {
            // Do nothing. Only for constructor call.
        }
    }
}
