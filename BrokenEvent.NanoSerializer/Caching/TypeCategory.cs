namespace BrokenEvent.NanoSerializer.Caching
{
  internal enum TypeCategory
  {
    Primitive,
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
