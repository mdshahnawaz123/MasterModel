using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DataLab;
using DataLab.Extensions;

[Transaction(TransactionMode.Manual)]
public class DoorOpeningLinkedModel : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uidoc = commandData.Application.ActiveUIDocument;
        var doc = uidoc.Document;

        try
        {
            var opt = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine
            };

            var doors = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .ToList();

            doc.DoAction(() =>
            {
                foreach (var door in doors)
                {
                    // Get the host wall directly from the door instance
                    if (door.Host is not Wall hostWall)
                        continue;

                    // Get the door's bounding box in world coordinates
                    var doorBB = door.get_BoundingBox(null);
                    if (doorBB == null)
                        continue;

                    var min = doorBB.Min;
                    var max = doorBB.Max;

                    // NewOpening(Wall, XYZ bottom, XYZ top) cuts a rectangular
                    // opening through the wall using two diagonal corner points
                    // expressed in the wall face's local plane
                    Opening opening = doc.Create.NewOpening(hostWall, min, max);
                }
            });

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}