using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO.Compression;

namespace SkinPackMaker
{
    public partial class Main : Form
    {
        public const string CustomBundleName = "RS_SHARED/customassets";
        public static Image NoImage;
        public static Image ErrorImage;
        public bool UnsavedChanges = false;
        public bool UnsavedPackChanges = false;
        public string CurrentFile;
        SkinControl LastSelected;
        List<SkinControl> SkinControls = new List<SkinControl>();
        List<MaterialProperty> MaterialProperties = new List<MaterialProperty>();
        List<MaterialProperty> HWMaterialProperties = new List<MaterialProperty>();
        public static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SkinData[]));
        public Main()
        {
            InitializeComponent();
            NoImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("SkinPackMaker.no-image.png"));
            ErrorImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("SkinPackMaker.error.png"));
            TypeSelector.Items.Clear();
            TypeSelector.Items.Add("(Custom)");
            foreach (var p in Constants.TypePresets.Keys)
                TypeSelector.Items.Add(p);
            TypeSelector.SelectedIndex = 0;
            UnsavedChanges = false;
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && args[1].ToLowerInvariant().EndsWith(".spproject") && File.Exists(args[1]))
                TryOpenFile(args[1]);
        }

        public void ValidateKeyNumericTextbox(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox box
                && !char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && !(e.KeyChar == '.' && !box.Text.Contains(".")))
                e.Handled = true;
        }

        public void PickColor(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                SelectColor.Color = button.BackColor;
                if (SelectColor.ShowDialog(this) == DialogResult.OK)
                    button.BackColor = SelectColor.Color;
            }
        }

        void SaveChanges()
        {
            if (!UnsavedChanges || LastSelected == null)
                return;
            UnsavedPackChanges = true;
            UnsavedChanges = false;
            LastSelected.data.Name = NameTextbox.Text;
            LastSelected.data.Id = (int)IDInput.Value;
            LastSelected.data.Icon = IconTextbox.Text;
            if (TypeSelector.SelectedIndex == 0)
            {
                LastSelected.data.TypePreset = null;
                LastSelected.data.Type = (int)TypeInput.Value;
                LastSelected.data.Renderers.Clear();
                foreach (var i in RendererList.Items)
                    LastSelected.data.Renderers.Add(i.ToString());
            }
            else
            {
                LastSelected.data.TypePreset = TypeSelector.Items[TypeSelector.SelectedIndex].ToString();
                LastSelected.data.Type = 0;
                LastSelected.data.Renderers.Clear();
            }
            LastSelected.data.BabyMesh = string.IsNullOrWhiteSpace(BabyBundleTextbox.Text) ? null : ((string, string)?)(BabyBundleTextbox.Text, BabyAssetTextbox.Text);
            LastSelected.data.TeenMesh = string.IsNullOrWhiteSpace(TeenBundleTextbox.Text) ? null : ((string, string)?)(TeenBundleTextbox.Text, TeenAssetTextbox.Text);
            LastSelected.data.AdultMesh = string.IsNullOrWhiteSpace(AdultBundleTextbox.Text) ? null : ((string, string)?)(AdultBundleTextbox.Text, AdultAssetTextbox.Text);
            LastSelected.data.TitanMesh = string.IsNullOrWhiteSpace(TitanBundleTextbox.Text) ? null : ((string, string)?)(TitanBundleTextbox.Text, TitanAssetTextbox.Text);
            LastSelected.data.Materials.Clear();
            foreach (var p in MaterialProperties)
            {
                LastSelected.data.Materials.Add(p.data);
                p.Save();
            }
            if (HWCheckbox.Checked)
            {
                LastSelected.data.HWMaterials.Clear();
                foreach (var p in HWMaterialProperties)
                {
                    LastSelected.data.HWMaterials.Add(p.data);
                    p.Save();
                }
            }
            else
                LastSelected.data.HWMaterials = null;
            LastSelected.RefreshControls();
        }

        public bool EnsureSaved()
        {
            if (!UnsavedChanges)
                return true;
            var r = MessageBox.Show(this, "There are unsaved changes to this skin, would you like to save them before editing a different skin?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Cancel)
                return false;
            if (r == DialogResult.Yes)
                SaveChanges();
            return true;
        }

        public void TrySelectSkin(SkinControl skin)
        {
            if (EnsureSaved())
                SelectSkin(skin);
        }

        void SelectSkin(SkinControl skin)
        {
            LastSelected = skin;
            if (skin == null)
            {
                MainPanel.Panel2.Enabled = false;
                RemoveSkinButton.Enabled = false;
                CopySkinButton.Enabled = false;
                goto end;
            }
            MainPanel.Panel2.Enabled = true;
            RemoveSkinButton.Enabled = true;
            CopySkinButton.Enabled = true;
            NameTextbox.Text = LastSelected.data.Name;
            IDInput.Value = LastSelected.data.Id;
            IconTextbox.Text = LastSelected.data.Icon;
            if (LastSelected.data.TypePreset == null)
            {
                TypeSelector.SelectedIndex = 0;
                TypeInput.Value = LastSelected.data.Type;
                RendererList.Items.Clear();
                foreach (var i in LastSelected.data.Renderers)
                    RendererList.Items.Add(i);
            }
            else
                TypeSelector.SetSelected(LastSelected.data.TypePreset);
            (BabyBundleTextbox.Text, BabyAssetTextbox.Text) = LastSelected.data.BabyMesh ?? ("", "");
            (TeenBundleTextbox.Text, TeenAssetTextbox.Text) = LastSelected.data.TeenMesh ?? ("", "");
            (AdultBundleTextbox.Text, AdultAssetTextbox.Text) = LastSelected.data.AdultMesh ?? ("", "");
            (TitanBundleTextbox.Text, TitanAssetTextbox.Text) = LastSelected.data.TitanMesh ?? ("", "");
            while (MaterialProperties.Count > 0)
            {
                MaterialProperties[0].Dispose();
                MaterialProperties.RemoveAt(0);
            }
            foreach (var m in LastSelected.data.Materials)
                MaterialProperties.Add(
                    Constants.TextureProperties.ContainsKey(m.Property) ? new TextureMaterialProperty(this,MaterialsLayout,m)
                    : Constants.ColorProperties.ContainsKey(m.Property) ? (MaterialProperty)new ColorMaterialProperty(this, MaterialsLayout, m)
                    : new FloatMaterialProperty(this, MaterialsLayout, m));
            while (HWMaterialProperties.Count > 0)
            {
                HWMaterialProperties[0].Dispose();
                HWMaterialProperties.RemoveAt(0);
            }
            if (HWCheckbox.Checked = LastSelected.data.HWMaterials != null)
                foreach (var m in LastSelected.data.HWMaterials)
                    HWMaterialProperties.Add(
                        Constants.TextureProperties.ContainsKey(m.Property) ? new TextureMaterialProperty(this, HWMaterialsLayout, m)
                        : Constants.ColorProperties.ContainsKey(m.Property) ? (MaterialProperty)new ColorMaterialProperty(this, HWMaterialsLayout, m)
                        : new FloatMaterialProperty(this, HWMaterialsLayout, m));
        end:
            UnsavedChanges = false;
        }

        void SaveClicked(object sender, EventArgs e) => SaveChanges();

        public void ControlChanged(object sender, EventArgs e) => UnsavedChanges = true;

        void ToggleHW(object sender, EventArgs e)
        {
            ControlChanged(sender, e);
            if (sender is CheckBox check)
            {
                HWPanel.Enabled = check.Checked;
                if (!check.Checked)
                    while (HWMaterialProperties.Count > 0)
                    {
                        HWMaterialProperties[0].Dispose();
                        HWMaterialProperties.RemoveAt(0);
                    }
            }
        }

        public void FileButton(object sender, EventArgs e)
        {
            if (sender is Control control && control.Parent.Controls[control.Tag?.ToString()] is TextBox text && Constants.OpenFileTypes.TryGetValue(text.Tag?.ToString(), out var filter))
            {
                OpenDialog.Filter = filter;
                OpenDialog.Title = text.Tag?.ToString();
                if (File.Exists(text.Text))
                    OpenDialog.FileName = text.Text;
                if (OpenDialog.ShowDialog(this) == DialogResult.OK)
                    text.Text = OpenDialog.FileName;
            }
        }

        void SelectTypePreset(object sender, EventArgs e)
        {
            var selected = TypeSelector.Items[TypeSelector.SelectedIndex].ToString();
            if (selected != "(Custom)" && Constants.TypePresets.TryGetValue(selected,out var v))
            {
                settingPreset = true;
                TypeInput.Value = v.Type;
                RendererList.Items.Clear();
                foreach (var i in v.Renderers)
                    RendererList.Items.Add(i);
                settingPreset = false;
            }
            ControlChanged(sender, e);
        }

        bool settingPreset = false;
        void PetTypeChanged(object sender, EventArgs e)
        {
            if (!settingPreset)
            {
                TypeSelector.SelectedIndex = 0;
                ControlChanged(sender, e);
            }
        }

        void AddRenderer(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(RendererTextbox.Text))
            {
                TypeSelector.SelectedIndex = 0;
                RendererList.Items.Add(RendererTextbox.Text);
                RendererTextbox.Text = "";
                ControlChanged(sender, e);
            }
        }

        void RemoveRenderer(object sender, EventArgs e)
        {
            if (RendererList.SelectedIndex >= 0)
            {
                TypeSelector.SelectedIndex = 0;
                RendererList.Items.RemoveAt(RendererList.SelectedIndex);
                ControlChanged(sender, e);
            }
        }

        void AddTextureButton(object sender, EventArgs e)
        {
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new TextureMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.TextureProperties.Keys.First() }));
        }

        void AddColorButton(object sender, EventArgs e)
        {
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new ColorMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.ColorProperties.Keys.First(), Value = "00000000" }));
        }

        void AddNumberButton(object sender, EventArgs e)
        {
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new FloatMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.FloatProperties.Keys.First(), Value = "0" }));
        }

        public void RemoveMaterialProperty(MaterialProperty property)
        {
            MaterialProperties.Remove(property);
            HWMaterialProperties.Remove(property);
            property.Dispose();
        }

        void AddNewSkin(object sender, EventArgs e)
        {
            if (EnsureSaved())
            {
                UnsavedPackChanges = true;
                var n = new SkinControl(this,SkinsLayout,new SkinData());
                SkinControls.Add(n);
                SelectSkin(n);
            }
        }

        void RemoveSkin(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,"Are you sure you want to remove this skin? This cannot be undone.","Confirm Remove",MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UnsavedPackChanges = true;
                SkinControls.Remove(LastSelected);
                LastSelected.Dispose();
                SelectSkin(SkinControls.FirstOrDefault());
            }
        }

        void DuplicateSkin(object sender, EventArgs e)
        {
            if (EnsureSaved())
            {
                UnsavedPackChanges = true;
                var n = new SkinControl(this, SkinsLayout, LastSelected.data.DeepMemberwiseClone());
                SkinControls.Add(n);
                SelectSkin(n);
            }
        }

        void DiscardChanges(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to discard changes to this skin? This cannot be undone.", "Confirm Discard", MessageBoxButtons.YesNo) == DialogResult.Yes)
                SelectSkin(LastSelected);
        }

        public bool EnsureFileSaved()
        {
            if (!EnsureSaved())
                return false;
            if (!UnsavedPackChanges)
                return true;
            var r = MessageBox.Show(this, "There are unsaved changes to this skin pack, would you like to save now?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Cancel)
                return false;
            if (r == DialogResult.Yes)
                return SaveFile();
            return true;
        }

        public bool SaveFile() => SaveFileAs(CurrentFile);

        public bool SaveFileAs(string filename)
        {
            if (filename == null)
            {
                SaveDialog.Filter = Constants.SaveFileTypes[SaveDialog.Title = "Save Project"];
                if (SaveDialog.ShowDialog(this) != DialogResult.OK)
                    return false;
                filename = SaveDialog.FileName;
            }
            try
            {
                using (var stream = File.Open(filename,FileMode.Create,FileAccess.Write,FileShare.None))
                    serializer.WriteObject(stream, SkinControls.Select(s => s.data).ToArray());
                CurrentFile = filename;
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occured while saving the file\n" + e, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        void NewFileClicked(object sender, EventArgs e)
        {
            if (EnsureFileSaved())
            {
                CurrentFile = null;
                LoadDatas(null);
            }
        }

        void LoadDatas(SkinData[] skins)
        {
            while (SkinControls.Count > 0)
            {
                SkinControls[0].Dispose();
                SkinControls.RemoveAt(0);
            }
            if (skins?.Length > 0)
                foreach (var s in skins)
                    SkinControls.Add(new SkinControl(this, SkinsLayout, s));
            SelectSkin(SkinControls.FirstOrDefault());
            UnsavedPackChanges = false;
        }

        void OpenFileClicked(object sender, EventArgs e)
        {
            if (EnsureFileSaved())
            {
                OpenDialog.Filter = Constants.OpenFileTypes[OpenDialog.Title = "Open Project"];
                if (OpenDialog.ShowDialog(this) == DialogResult.OK)
                    TryOpenFile(OpenDialog.FileName);
            }
        }

        public void TryOpenFile(string filename)
        {
            try
            {
                using (var stream = File.OpenRead(filename))
                    LoadDatas((SkinData[])serializer.ReadObject(stream));
                CurrentFile = filename;
            } catch (Exception e)
            {
                MessageBox.Show(this, "An error occured while loading the file\n"+e, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void SaveFileClicked(object sender, EventArgs e) => SaveFile();

        void SaveFileAsClicked(object sender, EventArgs e) => SaveFileAs(null);

        void ExportClicked(object sender, EventArgs e)
        {
            if (SkinControls.Count == 0)
            {
                MessageBox.Show(this, "There are no skins to export...");
                return;
            }
            if (!EnsureFileSaved())
                return;
            SaveDialog.Filter = Constants.SaveFileTypes[SaveDialog.Title = "Export Pack"];
            if (SaveDialog.ShowDialog(this) != DialogResult.OK)
                return;
            try { 
                var filename = SaveDialog.FileName;
                var simpleName = Path.GetFileNameWithoutExtension(filename);
                var tick = DateTime.UtcNow.Ticks;
                var packagedFiles = new Dictionary<string, (string, byte[])>();
                var assetPaths = new Dictionary<string, Dictionary<string,string>>();
                var imagePaths = new Dictionary<string, string>();
                var imagePathsSpecial = new Dictionary<string, string>();
                var allImages = new HashSet<string>();
                var ind = 0;
                void TryAddImage(string file, bool imageSpecialBundle = false)
                {
                    if (file == null || !File.Exists(file))
                        return;
                    var lFile = file.ToLowerInvariant();
                    var d = imageSpecialBundle ? imagePathsSpecial : imagePaths;
                    if (d.ContainsKey(lFile))
                        return;
                    if (allImages.Add(lFile))
                        packagedFiles[lFile] = ($"{ind}-{Path.GetFileName(file)}",File.ReadAllBytes(file));
                    if (imageSpecialBundle)
                        d[lFile] = $"{CustomBundleName}/{tick}-{simpleName} {ind++}-{Path.GetFileNameWithoutExtension(file)}";
                    else
                        d[lFile] = $"RS_SHARED/{tick}-{simpleName}/{ind++}-{Path.GetFileNameWithoutExtension(file)}";
                }
                void TryAddBundle(string file, string asset)
                {
                    if (!File.Exists(file))
                        return;
                    var lFile = file.ToLowerInvariant();
                    var d = assetPaths.GetOrCreate(lFile);
                    if (d.ContainsKey(asset))
                        return;
                    if (!packagedFiles.ContainsKey(lFile))
                        packagedFiles[lFile] = ($"{ind}-{Path.GetFileName(file)}", File.ReadAllBytes(file));
                    d[asset] = $"RS_SHARED/{tick}-{simpleName}/{ind++}-{asset}";
                }
                foreach (var s in SkinControls)
                {
                    TryAddImage(s.data.Icon,true);
                    if (s.data.BabyMesh != null)
                        TryAddBundle(s.data.BabyMesh.Value.Item1, s.data.BabyMesh.Value.Item2);
                    if (s.data.TeenMesh != null)
                        TryAddBundle(s.data.TeenMesh.Value.Item1, s.data.TeenMesh.Value.Item2);
                    if (s.data.AdultMesh != null)
                        TryAddBundle(s.data.AdultMesh.Value.Item1, s.data.AdultMesh.Value.Item2);
                    if (s.data.TitanMesh != null)
                        TryAddBundle(s.data.TitanMesh.Value.Item1, s.data.TitanMesh.Value.Item2);
                    foreach (var ms in new[] { s.data.Materials, s.data.HWMaterials })
                        if (ms != null)
                            foreach (var m in ms)
                                if (Constants.TextureProperties.ContainsKey(m.Property))
                                    TryAddImage(m.Value);
                }
                foreach (var p in allImages)
                {
                    var str = "";
                    if (imagePaths.TryGetValue(p, out var value))
                        str = $"asset\n{value.Before('/')}\n{value.After('/')}";
                    if (imagePathsSpecial.TryGetValue(p,out value))
                    {
                        if (str.Length != 0)
                            str += "\n\n";
                        str += $"asset\n{value.Before('/')}\n{value.After('/')}\n0";
                    }
                    packagedFiles[p + ".meta"] = (packagedFiles[p].Item1 + ".meta", Encoding.UTF8.GetBytes(str));
                }
                foreach (var b in assetPaths)
                {
                    var str = "";
                    foreach (var a in b.Value)
                    {
                        if (str.Length != 0)
                            str += "\n\n";
                        str += $"{a.Key}\nasset\n{a.Value.Before('/')}\n{a.Value.After('/')}";
                    }
                    packagedFiles[b.Key + ".meta"] = (packagedFiles[b.Key].Item1 + ".meta", Encoding.UTF8.GetBytes(str));
                }
                foreach (var s in SkinControls)
                {
                    var data = new SimpleResourceReplacer.CustomSkin();
                    data.Name = s.data.Name;
                    data.ItemID = s.data.Id;
                    data.SkinIcon = imagePathsSpecial.TryGetValue(s.data.Icon.ToLowerInvariant(), out var v) ? v : s.data.Icon;
                    
                    IList<string> renderers = s.data.Renderers;
                    var type = s.data.Type;
                    if (s.data.TypePreset != null)
                        (data.PetType, data.TargetRenderers) = Constants.TypePresets[s.data.TypePreset];
                    else
                    {
                        data.PetType = s.data.Type;
                        data.TargetRenderers = s.data.Renderers.ToArray();
                    }
                    if (s.data.BabyMesh != null || s.data.TeenMesh != null || s.data.AdultMesh != null || s.data.TitanMesh != null)
                    {
                        data.Mesh = new SimpleResourceReplacer.MeshOverrides();
                        if (s.data.BabyMesh != null)
                            data.Mesh.Baby = assetPaths.TryGetValue(s.data.BabyMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[s.data.BabyMesh.Value.Item2] : $"{s.data.BabyMesh.Value.Item1}/{s.data.BabyMesh.Value.Item2}";
                        if (s.data.TeenMesh != null)
                            data.Mesh.Baby = assetPaths.TryGetValue(s.data.TeenMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[s.data.TeenMesh.Value.Item2] : $"{s.data.TeenMesh.Value.Item1}/{s.data.TeenMesh.Value.Item2}";
                        if (s.data.AdultMesh != null)
                            data.Mesh.Adult = assetPaths.TryGetValue(s.data.AdultMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[s.data.AdultMesh.Value.Item2] : $"{s.data.AdultMesh.Value.Item1}/{s.data.AdultMesh.Value.Item2}";
                        if (s.data.TitanMesh != null)
                            data.Mesh.Titan = assetPaths.TryGetValue(s.data.TitanMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[s.data.TitanMesh.Value.Item2] : $"{s.data.TitanMesh.Value.Item1}/{s.data.TitanMesh.Value.Item2}";
                    }
                    data.MaterialData = s.data.Materials.Select(x => new SimpleResourceReplacer.MaterialProperty() {
                        Target = x.Age + (x.Part == Part.Both ? "" : x.Part.ToString()),
                        Property = x.Property,
                        Value = Constants.TextureProperties.ContainsKey(x.Property) && imagePaths.TryGetValue(x.Value.ToLowerInvariant(), out v) ? v : x.Value
                    }).ToArray();
                    if (s.data.HWMaterials != null)
                        data.HWMaterialData = s.data.HWMaterials.Select(x => new SimpleResourceReplacer.MaterialProperty()
                        {
                            Target = x.Age + (x.Part == Part.Both ? "" : x.Part.ToString()),
                            Property = x.Property,
                            Value = Constants.TextureProperties.ContainsKey(x.Property) && imagePaths.TryGetValue(x.Value.ToLowerInvariant(), out v) ? v : x.Value
                        }).ToArray();
                    packagedFiles[$"{ind}-{s.data.Name}.skin"] = ($"{ind++}-{s.data.Name}.skin", data.JsonSerialize());
                }
                using (var file = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var zip = new ZipArchive(file,ZipArchiveMode.Create,true,Encoding.UTF8))
                    foreach (var f in packagedFiles.Values)
                    { 
                        var entry = zip.CreateEntry(f.Item1, CompressionLevel.Optimal);
                        using (var stream = entry.Open())
                            stream.Write(f.Item2, 0, f.Item2.Length);
                    }
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "An error occured while saving the file\n" + err, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    [Serializable]
    public class SkinData
    {
        public string Name = "";
        public int Id;
        public string Icon = "";
        public List<string> Renderers = new List<string>();
        public int Type;
        public string TypePreset;
        public (string, string)? BabyMesh;
        public (string, string)? TeenMesh;
        public (string, string)? AdultMesh;
        public (string, string)? TitanMesh;
        public List<MaterialData> Materials = new List<MaterialData>();
        public List<MaterialData> HWMaterials;
    }

    public class SkinControl : Panel
    {
        public PictureBox img;
        public Label lbl;
        public readonly SkinData data;
        public SkinControl(Main main, Control parent, SkinData skin)
        {
            data = skin;
            Parent = parent;
            img = new PictureBox();
            img.Parent = this;
            lbl = new Label();
            lbl.Parent = this;

            Anchor = AnchorStyles.Top;
            AutoSize = true;
            BackColor = SystemColors.Control;
            Name = "SkinButton";
            Click += (x,y) => main.TrySelectSkin(this);
                
            img.Location = new Point(3, 3);
            img.Size = new Size(100, 100);
            img.Name = "SkinIcon";
            img.Click += (x, y) => main.TrySelectSkin(this);

            lbl.AutoSize = true;
            lbl.Location = new Point(3, 106);
            lbl.Margin = new Padding(3, 0, 3, 3);
            lbl.MinimumSize = lbl.MaximumSize = new Size(100, 0);
            lbl.Name = "SkinName";
            lbl.Click += (x, y) => main.TrySelectSkin(this);

            RefreshControls();
        }

        public void RefreshControls()
        {
            lbl.Text = data.Name;
            if (File.Exists(data.Icon))
                try
                {
                    img.Image = new Bitmap(data.Icon);
                }
                catch
                {
                    img.Image = Main.ErrorImage;
                }
            else
                img.Image = Main.NoImage;
        }
    }

    [Serializable]
    public class MaterialData
    {
        public Age Age;
        public Part Part;
        public string Property;
        public string Value;
    }

    public enum Age
    {
        Baby,
        Teen,
        Adult,
        Titan
    }

    public enum Part
    {
        Both,
        Eye,
        Body
    }

    public abstract class MaterialProperty : Panel
    {
        public ComboBox TargetAge;
        public ComboBox TargetPart;
        public ComboBox Property;
        public readonly MaterialData data;
        public MaterialProperty(Main main, Control parent, MaterialData material, IEnumerable<string> properties)
        {
            data = material;
            Parent = parent;
            TargetAge = new ComboBox();
            TargetAge.Parent = this;
            TargetPart = new ComboBox();
            TargetPart.Parent = this;
            Property = new ComboBox();
            Property.Parent = this;
            var Label = new Label();
            Label.Parent = this;
            var Btn = new Button();
            Btn.Parent = this;

            AutoSize = true;
            BackColor = SystemColors.Control;
            Name = "MaterialOption";

            TargetAge.FormattingEnabled = true;
            TargetAge.Location = new Point(3, 3);
            TargetAge.Size = new Size(150, 21);
            TargetAge.Name = "Age";
            TargetAge.SetContents(Enum.GetNames(typeof(Age)), data.Age.ToString());
            TargetAge.SelectedIndexChanged += main.ControlChanged;

            TargetPart.FormattingEnabled = true;
            TargetPart.Location = new Point(159, 3);
            TargetPart.Size = new Size(150, 21);
            TargetPart.Name = "Part";
            TargetPart.SetContents(Enum.GetNames(typeof(Part)), data.Part.ToString());
            TargetPart.SelectedIndexChanged += main.ControlChanged;

            Btn.Location = new Point(315, 3);
            Btn.Size = new Size(75, 23);
            Btn.UseVisualStyleBackColor = true;
            Btn.Text = "Remove";
            Btn.Name = "RemoveProperty";
            Btn.Click += (x, y) => main.RemoveMaterialProperty(this);

            Label.AutoSize = true;
            Label.Location = new Point(3, 33);
            Label.Text = "Property:";
            Label.Name = "PropertyLabel";

            Property.FormattingEnabled = true;
            Property.Location = new Point(58, 30);
            Property.Size = new Size(176, 21);
            Property.Name = "Property";
            Property.SetContents(properties, data.Property);
            Property.SelectedIndexChanged += main.ControlChanged;
        }

        public virtual void Save()
        {
            data.Age = (Age)Enum.Parse(typeof(Age), TargetAge.Items[TargetAge.SelectedIndex].ToString());
            data.Part = (Part)Enum.Parse(typeof(Part), TargetPart.Items[TargetPart.SelectedIndex].ToString());
            data.Property = Property.Items[Property.SelectedIndex].ToString();
        }
    }

    public class TextureMaterialProperty : MaterialProperty
    {
        public TextBox Input;
        public TextureMaterialProperty(Main main, Control parent, MaterialData material) : base(main,parent,material,Constants.TextureProperties.Keys)
        {
            Input = new TextBox();
            Input.Parent = this;
            var Btn = new Button();
            Btn.Parent = this;
            var Label = new Label();
            Label.Parent = this;

            Label.AutoSize = true;
            Label.Location = new Point(3, 60);
            Label.Text = "Texture:";
            Label.Name = "TextureLabel";

            Input.Location = new Point(58, 57);
            Input.MaxLength = 10000;
            Input.Size = new Size(265, 20);
            Input.Name = "TextureInput";
            Input.Text = data.Value;
            Input.Tag = "Select Image";
            Input.TextChanged += main.ControlChanged;

            Btn.Location = new Point(329, 55);
            Btn.Size = new Size(75, 23);
            Btn.UseVisualStyleBackColor = true;
            Btn.Text = "Browse...";
            Btn.Tag = "TextureInput";
            Btn.Name = "TextureButton";
            Btn.Click += main.FileButton;
        }

        public override void Save()
        {
            base.Save();
            data.Value = Input.Text;
        }
    }

    public class ColorMaterialProperty : MaterialProperty
    {
        public Button Btn;
        public ColorMaterialProperty(Main main, Control parent, MaterialData material) : base(main, parent, material, Constants.ColorProperties.Keys)
        {
            Btn = new Button();
            Btn.Parent = this;
            var Label = new Label();
            Label.Parent = this;

            Label.AutoSize = true;
            Label.Location = new Point(3, 60);
            Label.Text = "Color:";
            Label.Name = "ColorLabel";

            Btn.Location = new Point(58, 57);
            Btn.Size = new Size(23, 23);
            Btn.UseVisualStyleBackColor = false;
            Btn.Name = "ColorButton";
            Btn.BackColor = Color.FromArgb(uint.TryParse(material.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var c) ? c.AsInt() : 0);
            Btn.BackColorChanged += main.ControlChanged;
            Btn.Click += main.PickColor;
        }

        public override void Save()
        {
            base.Save();
            data.Value = $"{Btn.BackColor.ToArgb().AsUInt():X8}";
        }
    }

    public class FloatMaterialProperty : MaterialProperty
    {
        public TextBox Input;
        public FloatMaterialProperty(Main main, Control parent, MaterialData material) : base(main, parent, material, Constants.FloatProperties.Keys)
        {
            Input = new TextBox();
            Input.Parent = this;
            var Label = new Label();
            Label.Parent = this;

            Label.AutoSize = true;
            Label.Location = new Point(3, 60);
            Label.Text = "Value:";
            Label.Name = "FloatLabel";

            Input.Location = new Point(58, 57);
            Input.MaxLength = 10000;
            Input.Size = new Size(176, 20);
            Input.Name = "FloatInput";
            Input.Text = data.Value;
            Input.KeyPress += main.ValidateKeyNumericTextbox;
            Input.TextChanged += main.ControlChanged;
        }

        public override void Save()
        {
            base.Save();
            data.Value = Input.Text;
        }
    }

    public static class Constants
    {
        public static SortedDictionary<string, string> TextureProperties = new SortedDictionary<string, string>()
        {
            {"Normal Map","_BumpMap" },
            {"Colour Mask","_ColorMask" },
            {"Decal Map","_DecalMap" },
            {"Main Texture","_DetailTex" },
            {"Emissive Map","_EmissiveMap" },
            {"Glow Map","_MKGlowTex" },
            {"Specular Map","_SpecularMap" }
        };
        public static SortedDictionary<string, string> ColorProperties = new SortedDictionary<string, string>()
        {
            {"Emissive Colour","_EmissiveColor" },
            {"Glow Colour","_MKGlowColor" },
            {"Glow Texture Colour","_MKGlowTexColor" },
            {"Specular Colour","_SpecColor" }
        };
        public static SortedDictionary<string, string> FloatProperties = new SortedDictionary<string, string>()
        {
            {"Normal Strength","_BumpStrength" },
            {"Decal Opacity","_DecalOpacity" },
            {"Glossiness","_Glossiness" },
            {"Glow Power","_MKGlowPower" },
            {"Glow Texture Strength","_MKGlowTexStrength" }
        };
        public static Dictionary<string, string> OpenFileTypes = new Dictionary<string, string>()
        {
            {"Select Image","Images|*.png;*.jpg;*.jpeg" },
            {"Select Bundle","All files|*.*" },
            {"Open Project","Skin Pack Project|*.spproject" }
        };
        public static Dictionary<string, string> SaveFileTypes = new Dictionary<string, string>()
        {
            {"Save Project","Skin Pack Project|*.spproject" },
            {"Export Pack","Zip file|*.zip" }
        };
        public static SortedDictionary<string, (int Type, string[] Renderers)> TypePresets = new SortedDictionary<string, (int, string[])>()
        {
            { "Terrible Terror", (12, new[] { "TerribleTerror" })},
            { "Gronckle", (13, new[] { "Gronckle" })},
            { "Hideous Zippleback", (16, new[] { "Zippleback" })},
            { "Night Fury", (17, new[] { "NightFury" })},
            { "Deadly Nadder", (14, new[] { "Nadder" })},
            { "Monstrous Nightmare", (15, new[] { "Nightmare" })},
            { "Timberjack", (18, new[] { "Timberjack" })},
            { "Thunderdrum", (19, new[] { "Thunderdrum" })},
            { "Whispering Death", (20, new[] { "WhisperingDeath" })},
            { "Skrill", (21, new[] { "Skrill" })},
            { "Scauldron", (22, new[] { "Scauldron" })},
            { "Rumblehorn", (23, new[] { "RumbleHorn" })},
            { "Flightmare", (24, new[] { "DWFlightmare" })},
            { "Hobblegrunt", (25, new[] { "DWGeneric" })},
            { "Smothering Smokebreath", (26, new[] { "DWSmokeBreath" })},
            { "Typhoomerang", (27, new[] { "Typhoomerang" })},
            { "Raincutter", (28, new[] { "RainCutter" })},
            { "Boneknapper", (29, new[] { "Boneknapper", "BoneKnapper" })},
            { "Hotburple", (30, new[] { "DWHotBurple" })},
            { "Stormcutter", (31, new[] { "Stormcutter" })},
            { "Snafflefang", (32, new[] { "DWSnafflefang" })},
            { "Changewing", (33, new[] { "DWChangewing" })},
            { "Fireworm Queen", (34, new[] { "DWFirewormQueen" })},
            { "Screaming Death", (35, new[] { "DWScreamingDeath" })},
            { "Tide Glider", (37, new[] { "DWTideGlider","DWTideglider" })},
            { "Scuttleclaw", (38, new[] { "DWScuttleclaw" })},
            { "Sand Wraith", (39, new[] { "SandWraith" })},
            { "Sweet Death", (42, new[] { "DWSweetDeath" })},
            { "Woolly Howl", (43, new[] { "WoollyHowl" })},
            { "Shivertooth", (44, new[] { "Shivertooth" })},
            { "Groncicle", (45, new[] { "Groncicle" })},
            { "Speed Stinger", (46, new[] { "DWSpeedStinger" })},
            { "Shockjaw", (47, new[] { "Shockjaw" })},
            { "Moldruffle", (50, new[] { "DWMudruffle" })},
            { "Mudraker", (51, new[] { "DWMudraker" })},
            { "Deathsong", (52, new[] { "DWDeathsong","DWDeathSong" })},
            { "Razorwhip", (53, new[] { "DWRazorwhip" })},
            { "Grapple Grounder", (54, new[] { "DWGrappleGrounder","DWGrapplegrounder" })},
            { "Snow Wraith", (55, new[] { "DWSnowWraith" })},
            { "Prickleboggle", (58, new[] { "DWNettlewing" })},
            { "Sliquifier", (59, new[] { "DWSliquifier" })},
            { "Devilish Dervish", (61, new[] { "DWDevilishDervish" })},
            { "Snaptrapper", (62, new[] { "DWSnaptrapper" })},
            { "Quaken", (64, new[] { "DWQuaken" })},
            { "Thunderpede", (66, new[] { "DWThunderpede" })},
            { "NightTerror", (69, new[] { "NightTerror" })},
            { "Armorwing", (70, new[] { "DWArmorwing","DWArmorWing" })},
            { "Slithersong", (72, new[] { "Slithersong" })},
            { "Shovelhelm", (71, new[] { "DWShovelhelm" })},
            { "Windwalker", (76, new[] { "DWWindwalker" })},
            { "Eruptodon", (77, new[] { "DWEruptodon" })},
            { "Singetail", (78, new[] { "DWSingetail" })},
            { "Silver Phantom", (79, new[] { "DWSilverPhantom" })},
            { "Buffalord", (80, new[] { "DWBuffalord" })},
            { "Flame Whipper", (81, new[] { "FlameWhipper" })},
            { "Triple Stryke", (82, new[] { "DWTripleStryke" })},
            { "Sentinel", (91, new[] { "DWSentinel" })},
            { "Elder Sentinel", (92, new[] { "DWSentinel" })},
            { "Grim Gnasher", (93, new[] { "DWGrimGnasher" })},
            { "Dramillion", (94, new[] { "DWDramillion" })},
            { "Fire Terror", (95, new[] { "FireTerror" })},
            { "Deathgripper", (97, new[] { "DeathGripper" })},
            { "Light Fury (Membership)", (96, new[] { "LightFury" })},
            { "Crimson Goregutter", (98, new[] { "DWGoregutter" })},
            { "Hobgobbler", (99, new[] { "DWHobgobbler" })},
            { "Hobgobbler (Special)", (100, new[] { "DWHobgobbler" })},
            { "Skrillknapper", (101, new[] { "DWSkrillKnapper" })},
            { "Dreadstrider", (102, new[] { "DWFlightStinger" })},
            { "Night Light (Dart)", (103, new[] { "DWNightlightMesh", "DWDartAdultMesh" })},
            { "Night Light (Ruffrunner)", (104, new[] { "DWNightlightMesh", "DWRuffrunnerAdultMesh" })},
            { "Night Light (Pouncer)", (105, new[] { "DWNightlightMesh", "DWPouncerAdultMesh" })},
            { "Ghastly Zapplejack", (106, new[] { "DWZapplejack" })},
            { "Deathly Galeslash", (107, new[] { "DWGaleslash" })},
            { "Ridgesnipper", (108, new[] { "DWRidgesnipper" })},
            { "Abomibumble", (109, new[] { "DWAbomibumble" })},
            { "Bonestormer", (110, new[] { "DWBonestormer" })},
            { "Chimeragon", (111, new[] { "DWChimeragon" })},
            { "Slitherwing", (112, new[] { "DWSlitherwing" })},
            { "Seastormer", (113, new[] { "DWSeastormer" })},
            { "CavernCrasher", (114, new[] { "DWCavernCrasher" })},
            { "Humbanger", (115, new[] { "DWHumbanger" })},
            { "Golden Dragon", (116, new[] { "DWGoldenDragon" })},
            { "Hushboggle", (117, new[] { "DWHushboggle" })},
            { "Zipplewraith", (118, new[] { "DWZipplewraith" })},
            { "Gruesome Goregripper", (119, new[] { "DWGoregripper" })},
            { "Graveknapper", (120, new[] { "DWGraveknapper" })},
            { "Frostmare", (121, new[] { "DWFrostmare" })},
            { "Songwing", (123, new[] { "DWSongwing" })},
            { "Sandbuster", (122, new[] { "Sandbuster", "Spikes" })},
            { "Sword Stealer", (125, new[] { "DWSwordstealer", "DragonSpikesMesh" })},
            { "Light Fury", (126, new[] { "DWLightfury", "DWNightlight" })},
            { "Crimson Holwer", (128, new[] { "PfDWCrimsonHolwer" })}
        };
    }

    public static class ExtentionMethods
    {
        public static void SetContents(this ComboBox combo, IEnumerable<string> items, string selected)
        {
            combo.Items.Clear();
            var ind = 0;
            var flag = false;
            foreach (var p in items)
            {
                combo.Items.Add(p);
                if (flag)
                    continue;
                else if (p == selected)
                    flag = true;
                else
                    ind++;
            }
            if (ind == combo.Items.Count)
                ind = 0;
            combo.SelectedIndex = ind;
        }
        public static void SetSelected(this ComboBox combo, string selected)
        {
            var ind = 0;
            var flag = false;
            foreach (var p in combo.Items)
            {
                if (flag)
                    continue;
                else if (p as string == selected)
                    flag = true;
                else
                    ind++;
            }
            if (ind == combo.Items.Count)
                ind = 0;
            combo.SelectedIndex = ind;
        }
        public static unsafe int AsInt(this uint v) => (int)(void*)v;
        public static unsafe uint AsUInt(this int v) => (uint)(void*)v;

        public static T DeepMemberwiseClone<T>(this T obj) => obj.DeepMemberwiseClone(new Dictionary<object, object>(), new HashSet<object>());
        static T DeepMemberwiseClone<T>(this T obj, Dictionary<object, object> cloned, HashSet<object> created)
        {
            if (obj == null)
                return obj;
            if (cloned.TryGetValue(obj, out var clone))
                return (T)clone;
            if (created.Contains(obj))
                return obj;
            var t = obj.GetType();
            if (t.IsPrimitive || t == typeof(string))
                return obj;
            if (t.IsArray && obj is Array a)
            {
                var c = t.GetConstructors()[0];
                var o = new object[t.GetArrayRank()];
                for (int i = 0; i < o.Length; i++)
                    o[i] = a.GetLength(i);
                var na = (Array)c.Invoke(o);
                created.Add(na);
                cloned[a] = na;
                for (int i = 0; i < o.Length; i++)
                    if ((int)o[i] == 0)
                        return (T)(object)na;
                var ind = new int[o.Length];
                var flag = true;
                while (flag)
                {
                    na.SetValue(a.GetValue(ind).DeepMemberwiseClone(cloned, created), ind);
                    for (int i = 0; i < ind.Length; i++)
                    {
                        ind[i]++;
                        if (ind[i] == (int)o[i])
                        {
                            if (i == ind.Length - 1)
                                flag = false;
                            ind[i] = 0;
                        }
                        else
                            break;
                    }
                }
                return (T)(object)na;
            }
            var nObj = (T)FormatterServices.GetUninitializedObject(t);
            created.Add(nObj);
            cloned[obj] = nObj;
            var b = typeof(object);
            while (t != b)
            {
                foreach (var f in t.GetFields(~BindingFlags.Default))
                    if (!f.IsStatic)
                        f.SetValue(nObj, f.GetValue(obj).DeepMemberwiseClone(cloned, created));
                t = t.BaseType;
            }
            return nObj;
        }
        public static Y GetOrCreate<X, Y>(this IDictionary<X, Y> d, X key) where Y : new()
        {
            if (d.TryGetValue(key, out var value))
                return value;
            return d[key] = new Y();
        }
        public static string After(this string original, char delimeter, bool inclusive = false)
        {
            var split = original.LastIndexOf(delimeter);
            if (split == -1)
                return original;
            return original.Remove(0, split + (inclusive ? 0 : 1));
        }
        public static string Before(this string original, char delimeter, bool inclusive = false)
        {
            var split = original.LastIndexOf(delimeter);
            if (split == -1)
                return original;
            return original.Remove(split + (inclusive ? 1 : 0));
        }
        public static byte[] Serialize(this XmlObjectSerializer serializer, object graph, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);
                return stream.ToArray();
            }
        }
        public static byte[] JsonSerialize(this object graph, Encoding encoding = null) => new DataContractJsonSerializer(graph.GetType()).Serialize(graph,encoding);
    }
}
