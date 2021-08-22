namespace Prism
{
	internal record TreeListViewEntry(string Key, object Value, params TreeListViewEntry[] Children);
}