namespace BrokenEvent.NanoSerializer.Caching
{
  internal enum TypeCategory
  {
    Primitive,
    Enum,
    Unknown,
    // containers
    Array,
    IList,
    GenericIList,
    Queue,
    GenericQueue,
    Stack,
    GenericStack,
    IDictionary,
    ISet,
    LinkedList,
  }
}
