using System.Buffers.Text;
using System.Reflection;
using System.Reflection.Emit;
using Network.data;

namespace Network.Automation
{
    public static class Controller
    {
        private static List<Type> SimpleNetTypes = new List<Type>();
        public static List<Type> GetSimpleNetTypes()
        {
            return SimpleNetTypes;
        }
        public static void CreateNetType<netType>(string name) where netType : NetType
        {
            // Create new class type
            AssemblyName assemblyName = new AssemblyName(name);
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? name);
            TypeBuilder builder = moduleBuilder.DefineType(name, TypeAttributes.Public|TypeAttributes.Class, typeof(SimpleClient<netType>));
            // Create the essential constructor
            Type[] parametors = new Type[] { typeof(SocketData) };
            ConstructorBuilder constructor = builder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                parametors);
            // Sets constructor code
            ILGenerator generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            ConstructorInfo? baseConstructor = typeof(SimpleClient<netType>).GetConstructor(parametors);
            generator.Emit(OpCodes.Call, baseConstructor!);
            generator.Emit(OpCodes.Ret);
            // Returns the new type
            Type clienttype = builder.CreateType();
            SimpleNetTypes.Add(clienttype);
        }
    }
    public abstract class SimpleClient<NetTopology> : Client<NetTopology> where NetTopology : NetType
    {
        public SimpleClient(SocketData data) : base(data) { Handles = new Dictionary<int, PacketScripts>() { { 0, CurrentMatch.Send } }; }
    }
}