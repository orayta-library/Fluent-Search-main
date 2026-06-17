# מדריך מפורט: יצירת ממשק חיפוש בסגנון Fluent Search באמצעות Avalonia

מדריך זה מסביר שלב-אחר-שלב כיצד ליצור ממשק חיפוש מודרני (Search box, Results list, Preview pane, ניווט מקלדת, תמות, ונגישות) עבור אפליקציה ב-.NET באמצעות Avalonia UI — בדומה לממשק של Fluent Search.

**קהל יעד:** מפתחי C#/.NET המעוניינים להטמיע חיפוש עם תצוגת תוצאות מתקדמת.

**מה תלמד כאן:**
- איך להקים פרויקט Avalonia בסיסי
- ארכיטקטורת MVVM לכל רכיבי החיפוש
- דוגמאות XAML ל-Layout (Search box, Results List, Preview Pane)
- ViewModel ודוגמת קוד חיפוש אסינכרוני
- יצירת `ResultPreview` מותאם אישית ב-Avalonia
- ניווט מקלדת, קיצורי דרך ונגישות
- התאמה לעיצוב ותמות (theme)
- שילוב בממשק קיים/שימוש כרכיב

**דרישות מוקדמות:**
- .NET SDK מתאים (6/7/8)
- Avalonia templates: `dotnet new install Avalonia.Templates` (אם אין)

**קישורים שימושיים מהריפו:**
- תיעוד Result Preview של Fluent Search: [Result Preview UI](docs/Plugins/CSharp/API/ResultPreviewUI.md)

**1. יצירת פרויקט חדש (Boilerplate)**

הרץ בשורת הפקודה:

```bash
dotnet new avalonia.app -o MySearchApp
cd MySearchApp
```

הוסף חבילות שימושיות (אם צריך):

```bash
dotnet add package Avalonia.ReactiveUI
dotnet add package CommunityToolkit.Mvvm
```

הסבר קצר: נשתמש ב-MVVM (CommunityToolkit.Mvvm) כדי להפריד לוגיקה מ-UI, ו-Avalonia ל-XAML מותאם שולחן עבודה.

**2. ארכיטקטורה מוצעת וקבצים**

- `App.axaml` / `App.axaml.cs` — משאבים ותמות
- `MainWindow.axaml` / `MainWindow.axaml.cs` — פריסת החלון הראשי
- `ViewModels/SearchViewModel.cs` — לוגיקת חיפוש
- `Models/SearchResult.cs` — ייצוג תוצאת חיפוש
- `Controls/ResultPreview.axaml` / `.cs` — רכיב תצוגה לכל תוצאה

**3. XAML — פריסת חלון ראשי (MainWindow.axaml)**

דוגמה ל-Layout בסיסי: שורת חיפוש עליונה, רשימת תוצאות משמאל, פאנל תצוגה מימין.

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:MySearchApp.ViewModels"
        xmlns:controls="clr-namespace:MySearchApp.Controls"
        x:Class="MySearchApp.MainWindow"
        Width="1000" Height="600">

  <Window.DataContext>
    <vm:SearchViewModel />
  </Window.DataContext>

  <Grid ColumnDefinitions="Auto,*,2*">
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto" />
      <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <!-- Search box -->
    <TextBox Grid.Row="0" Grid.Column="0" Grid.ColumnSpan="3"
             Watermark="Search..."
             Text="{Binding Query, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>

    <!-- Results list -->
    <ListBox Grid.Row="1" Grid.Column="0"
             Items="{Binding Results}"
             SelectedItem="{Binding SelectedResult, Mode=TwoWay}"
             Width="320">
      <ListBox.ItemTemplate>
        <DataTemplate>
          <StackPanel Orientation="Horizontal" Margin="6">
            <Image Source="{Binding Thumbnail}" Width="48" Height="48" />
            <StackPanel Margin="8,0">
              <TextBlock Text="{Binding Title}" FontWeight="Bold" />
              <TextBlock Text="{Binding Subtitle}" FontSize="12" />
            </StackPanel>
          </StackPanel>
        </DataTemplate>
      </ListBox.ItemTemplate>
    </ListBox>

    <!-- Preview pane -->
    <controls:ResultPreview Grid.Row="1" Grid.Column="1" Grid.ColumnSpan="2"
                            DataContext="{Binding SelectedResult}" />
  </Grid>
</Window>
```

הערה: ניתן להחליף את `ListBox` ב-`ItemsControl` + סגנון ריצוף להתאמה טובה יותר ל-CSS-like design.

**4. Model ו-ViewModel — דוגמאות קוד (C#)**

Models/SearchResult.cs

```csharp
using Avalonia.Media.Imaging;

public class SearchResult
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public Bitmap Thumbnail { get; set; }
    public string Path { get; set; }
}
```

ViewModels/SearchViewModel.cs

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

public partial class SearchViewModel : ObservableObject
{
    [ObservableProperty]
    private string query;

    [ObservableProperty]
    private ObservableCollection<SearchResult> results = new();

    [ObservableProperty]
    private SearchResult selectedResult;

    public SearchViewModel()
    {
        SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync);
    }

    public IAsyncRelayCommand SearchCommand { get; }

    private async Task ExecuteSearchAsync()
    {
        await Task.Delay(150);
        Results.Clear();
        if (string.IsNullOrWhiteSpace(Query)) return;
        for (int i = 0; i < 15; i++)
        {
            Results.Add(new SearchResult { Title = $"{Query} - Result {i+1}", Subtitle = "Sample subtitle" });
        }
        SelectedResult = Results.FirstOrDefault();
    }
}
```

כדאי להוסיף debounce על `Query` (Reactive Extensions או טיימר) כדי לא להפעיל חיפוש על כל הקלדה.

**5. יצירת `ResultPreview` מותאם (Controls/ResultPreview.axaml)**

ResultPreview.axaml:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="MySearchApp.Controls.ResultPreview">
  <Border Padding="12">
    <StackPanel>
      <TextBlock Text="{Binding Title}" FontSize="20" FontWeight="Bold" />
      <TextBlock Text="{Binding Subtitle}" Opacity="0.8" />
      <Image Source="{Binding Thumbnail}" Height="240" Stretch="Uniform" Margin="0,8" />
      <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
        <Button Content="Open" Command="{Binding OpenCommand}" />
        <Button Content="Copy Path" Command="{Binding CopyPathCommand}" Margin="8,0,0,0" />
      </StackPanel>
    </StackPanel>
  </Border>
</UserControl>
```

הפעלת פקודות יכולה להיות באמצעות `SelectedResult` שמכיל יתר על המידה `ICommand` או ע״י שידור אירוע/קריאה ל-ViewModel הראשי.

**6. ניווט מקלדת & קיצורי דרך**

```xml
<Window.InputBindings>
  <KeyBinding Gesture="Up" Command="{Binding SelectPreviousCommand}" />
  <KeyBinding Gesture="Down" Command="{Binding SelectNextCommand}" />
  <KeyBinding Gesture="Enter" Command="{Binding ActivateSelectedCommand}" />
</Window.InputBindings>
```

להוספת חיפוש גלובלי (hotkey עולמי), יש להיעזר ב-Platform-specific API (Windows global hotkeys) או בספריות חיצוניות.

**7. נגישות (Accessibility)**

השתמש ב-`AutomationProperties` וודא שכל כפתור ופאנל בעלי שם ותיאור.

```xml
<TextBox AutomationProperties.Name="Search box"/>
<ListBox AutomationProperties.Name="Search results"/>
```

**8. תמות ועיצוב (Styling & Theme)**

- שמור צבעים וריסורסים ב-`App.axaml` כדי לאפשר החלפה בין Light/Dark.

**9. ביצועים**

- השתמש ב-virtualization עבור רשימות ארוכות.
- טען תמונות באופן אסינכרוני והצג תצוגות ממוזערות.

**10. שילוב בממשק קיים**

- ארוז את ה-Controls שלך כ-Library (DLL) עם רכיבים עצמאיים ו-ViewModels שניתן להתקין באפליקציות אחרות.

---
אם תרצה, אמשיך ואוסיף דוגמאות קוד נוספות: debounce ל-Query, דוגמת אינדקס חיפוש (Lucene.NET), או פרויקט Example מלא ב-`samples/MySearchApp`.
