using System.IO;
using Loader.Components;

namespace Loader.Scanners
{
    public interface INodeScanner
    {
        Node Scan(Stream stream);
    }
}
