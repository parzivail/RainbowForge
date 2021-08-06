using CommandLine;

namespace DumpTool
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Parser.Default.ParseArguments<ListCommand, FindCommand, InspectCommand, DumpCommand, DumpMeshPropsCommand, IndexCommand,
					DumpAllCommand, DumpAllMeshPropsCommand, FindAllMeshPropsCommand, FindAllMeshPropsGlobalCommand>(args)
				.WithParsed<ListCommand>(ListCommand.Run)
				.WithParsed<FindCommand>(FindCommand.Run)
				.WithParsed<InspectCommand>(InspectCommand.Run)
				.WithParsed<DumpCommand>(DumpCommand.Run)
				.WithParsed<DumpAllCommand>(DumpAllCommand.Run)
				.WithParsed<DumpMeshPropsCommand>(DumpMeshPropsCommand.Run)
				.WithParsed<DumpAllMeshPropsCommand>(DumpAllMeshPropsCommand.Run)
				.WithParsed<FindAllMeshPropsCommand>(FindAllMeshPropsCommand.Run)
				.WithParsed<FindAllMeshPropsGlobalCommand>(FindAllMeshPropsGlobalCommand.Run)
				.WithParsed<IndexCommand>(IndexCommand.Run);
		}
	}
}