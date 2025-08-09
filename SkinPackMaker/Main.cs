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
using System.Configuration;
using static SimpleResourceReplacer.CustomSkin;

namespace SkinPackMaker
{
    public partial class Main : Form
    {
        public const string VERSION = "1.3.5.0";
        public const string CustomBundleName = "RS_SHARED/customassets";
        public static Image NoImage;
        public static Image ErrorImage;
        public bool UnsavedChanges = false;
        public bool UnsavedPackChanges = false;
        public string CurrentFile;
        public string LastPackExport;
        EquipmentControl LastSelected;
        public List<EquipmentControl> EquipmentControls = new List<EquipmentControl>();
        public List<MaterialProperty> MaterialProperties = new List<MaterialProperty>();
        public List<MaterialProperty> HWMaterialProperties = new List<MaterialProperty>();
        public static DataContractJsonSerializer equipmentSerializer = new DataContractJsonSerializer(typeof(EquipmentData[]),new[] { typeof(SkinData),typeof(SaddleData) });
        public static DataContractJsonSerializer oldSkinSerializer = new DataContractJsonSerializer(typeof(SkinData[]));
        public static int LastCreatorID
        {
            get => Settings.Instance.LastCreatorID;
            set
            {
                Settings.Instance.LastCreatorID = value;
                Settings.Save();
            }
        }
        public Main()
        {
            InitializeComponent();
            Text += " - " + VERSION;
            NoImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("SkinPackMaker.no-image.png"));
            ErrorImage = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("SkinPackMaker.error.png"));
            TypeSelector.Items.Clear();
            TypeSelector.Items.Add("(Custom)");
            foreach (var p in Constants.TypePresets.Keys)
                TypeSelector.Items.Add(p);
            TypeSelector.SelectedIndex = 0;
            foreach (var c in new[] { BabyBodyShaderCombobox, BabyEyesShaderCombobox, TeenBodyShaderCombobox, TeenEyesShaderCombobox, AdultBodyShaderCombobox, AdultEyesShaderCombobox, TitanBodyShaderCombobox, TitanEyesShaderCombobox })
                c.SetContents(Enum.GetNames(typeof(ShaderTypes)), ShaderTypes.Default.ToString());
            foreach (var c in new[] { BabyExtraShaderCombobox, TeenExtraShaderCombobox, AdultExtraShaderCombobox, TitanExtraShaderCombobox })
                c.SetContents(Enum.GetNames(typeof(ShaderTypes)), ShaderTypes.Extra.ToString());
            RequiredAgeCombobox.SetContents(Enum.GetNames(typeof(Age)).Select(x => x.ToUpperInvariant()), Age.Teen.ToString().ToUpperInvariant());
            UnsavedChanges = false;
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && args[1].ToLowerInvariant().EndsWith(".spproject") && File.Exists(args[1]))
                TryOpenFile(args[1]);
            FormClosing += (x, y) =>
            {
                if (!EnsureFileSaved())
                    y.Cancel = true;
            };
            
        }

        public void ValidateKeyNumericTextbox(object sender, KeyPressEventArgs e)
        {
            if (sender is TextBox box
                && !char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && !(e.KeyChar == '.' && !box.Text.Contains(".")))
                e.Handled = true;
        }

        ColorSelector SelectColor = new ColorSelector();
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
            LastSelected.data.Save(this);
            LastSelected.RefreshControls();
        }

        public bool EnsureSaved()
        {
            if (!UnsavedChanges)
                return true;
            var r = MessageBox.Show(this, "There are unsaved changes to this skin, would you like to save them now? Unsaved changes will be lost", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Cancel)
                return false;
            if (r == DialogResult.Yes)
                SaveChanges();
            return true;
        }

        public void TrySelectEquipment(EquipmentControl equipment)
        {
            if (EnsureSaved())
                SelectEquipment(equipment);
        }

        void SelectEquipment(EquipmentControl equipment)
        {
            LastSelected = equipment;
            SuspendLayout();
            SkinPanel.Visible = false;
            SaddlePanel.Visible = false;
            if (equipment == null)
            {
                MainPanel.Panel2.Enabled = false;
                RemoveSkinButton.Enabled = false;
                CopySkinButton.Enabled = false;
                goto end;
            }
            MainPanel.Panel2.Enabled = true;
            RemoveSkinButton.Enabled = true;
            CopySkinButton.Enabled = true;
            equipment.data.Select(this);
        end:
            ResumeLayout();
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
                var dialog = OpenDialog;
                dialog.Filter = filter;
                dialog.Title = text.Tag?.ToString();
                dialog.TrySetFile(text.Text);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    text.Text = dialog.FileName;
            }
        }

        void SelectTypePreset(object sender, EventArgs e)
        {
            var selected = TypeSelector.GetSelected();
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
            SuspendLayout();
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new TextureMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.TextureProperties.Keys.First() }));
            ResumeLayout();
            ControlChanged(sender, e);
        }

        void AddColorButton(object sender, EventArgs e)
        {
            SuspendLayout();
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new ColorMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.ColorProperties.Keys.First(), Value = Constants.ColorProperties.Values.First().value.ToHex() }));
            ResumeLayout();
            ControlChanged(sender, e);
        }

        void AddNumberButton(object sender, EventArgs e)
        {
            SuspendLayout();
            if (sender is Control control)
                (control.Tag as string == "HW" ? HWMaterialProperties : MaterialProperties)
                    .Add(new FloatMaterialProperty(
                        this,
                        control.Tag as string == "HW" ? HWMaterialsLayout : MaterialsLayout,
                        new MaterialData() { Property = Constants.FloatProperties.Keys.First(), Value = Constants.FloatProperties.Values.First().value.ToString() }));
            ResumeLayout();
            ControlChanged(sender, e);
        }

        public void RemoveMaterialProperty(MaterialProperty property)
        {
            MaterialProperties.Remove(property);
            HWMaterialProperties.Remove(property);
            property.Dispose();
            ControlChanged(null, null);
        }

        void AddNewSkin(object sender, EventArgs e)
        {
            if (EnsureSaved())
            {
                UnsavedPackChanges = true;
                SuspendLayout();
                var n = new EquipmentControl(this,EquipmentLayout,new SkinData());
                ResumeLayout();
                EquipmentControls.Add(n);
                SelectEquipment(n);
            }
        }

        void AddNewSaddle(object sender, EventArgs e)
        {
            if (EnsureSaved())
            {
                UnsavedPackChanges = true;
                SuspendLayout();
                var n = new EquipmentControl(this, EquipmentLayout, new SaddleData());
                ResumeLayout();
                EquipmentControls.Add(n);
                SelectEquipment(n);
            }
        }

        void RemoveEquipment(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,"Are you sure you want to remove this skin? This cannot be undone.","Confirm Remove",MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                UnsavedPackChanges = true;
                EquipmentControls.Remove(LastSelected);
                LastSelected.Dispose();
                SelectEquipment(EquipmentControls.FirstOrDefault());
            }
        }

        void DuplicateEquipment(object sender, EventArgs e)
        {
            if (EnsureSaved())
            {
                UnsavedPackChanges = true;
                SuspendLayout();
                var n = new EquipmentControl(this, EquipmentLayout, LastSelected.data.DeepMemberwiseClone());
                ResumeLayout();
                EquipmentControls.Add(n);
                SelectEquipment(n);
            }
        }

        void DiscardChanges(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to discard changes to this skin? This cannot be undone.", "Confirm Discard", MessageBoxButtons.YesNo) == DialogResult.Yes)
                SelectEquipment(LastSelected);
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
                SaveDialog.TrySetFile(CurrentFile);
                if (SaveDialog.ShowDialog(this) != DialogResult.OK)
                    return false;
                filename = SaveDialog.FileName;
            }
            try
            {
                using (var stream = File.Open(filename,FileMode.Create,FileAccess.Write,FileShare.None))
                    equipmentSerializer.WriteObject(stream, EquipmentControls.Select(s => s.data).ToArray());
                UnsavedPackChanges = false;
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

        void LoadDatas(EquipmentData[] data)
        {
            if (data?.Length > 0)
                foreach (var item in data)
                if (item.TypePreset != null && Constants.PresetNameCorrects.TryGetValue(item.TypePreset, out var correction))
                    item.TypePreset = correction;
            SuspendLayout();
            while (EquipmentControls.Count > 0)
            {
                EquipmentControls[0].Dispose();
                EquipmentControls.RemoveAt(0);
            }
            if (data?.Length > 0)
                foreach (var s in data)
                    EquipmentControls.Add(new EquipmentControl(this, EquipmentLayout, s));
            SelectEquipment(EquipmentControls.FirstOrDefault());
            ResumeLayout();
            UnsavedPackChanges = false;
        }

        void OpenFileClicked(object sender, EventArgs e)
        {
            if (EnsureFileSaved())
            {
                OpenDialog.Filter = Constants.OpenFileTypes[OpenDialog.Title = "Open Project"];
                OpenDialog.TrySetFile(CurrentFile);
                if (OpenDialog.ShowDialog(this) == DialogResult.OK)
                    TryOpenFile(OpenDialog.FileName);
            }
        }

        public void TryOpenFile(string filename)
        {
            try
            {
                using (var stream = File.OpenRead(filename))
                    LoadDatas((EquipmentData[])equipmentSerializer.ReadObject(stream));
                CurrentFile = filename;
            }
            catch (Exception e)
            {
                try
                {
                    using (var stream = File.OpenRead(filename))
                        LoadDatas((SkinData[])oldSkinSerializer.ReadObject(stream));
                    CurrentFile = filename;
                }
                catch
                {
                    MessageBox.Show(this, "An error occured while loading the file\n" + e, "File Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void SaveFileClicked(object sender, EventArgs e) => SaveFile();

        void SaveFileAsClicked(object sender, EventArgs e) => SaveFileAs(null);

        void ExportClicked(object sender, EventArgs e)
        {
            if (EquipmentControls.Count == 0)
            {
                MessageBox.Show(this, "There are no skins to export...");
                return;
            }
            if (!EnsureFileSaved())
                return;
            SaveDialog.Filter = Constants.SaveFileTypes[SaveDialog.Title = "Export Pack"];
            SaveDialog.TrySetFile(LastPackExport);
            if (SaveDialog.ShowDialog(this) != DialogResult.OK)
                return;
            try { 
                var filename = LastPackExport = SaveDialog.FileName;
                var simpleName = Path.GetFileNameWithoutExtension(filename);
                var tick = DateTime.UtcNow.Ticks;
                var packagedFiles = new Dictionary<string, (string, byte[])>();
                var assetPaths = new Dictionary<string, Dictionary<string,string>>();
                var imagePaths = new Dictionary<string, string>();
                var imagePathsSpecial = new Dictionary<string, string>();
                var allImages = new HashSet<string>();
                var ind = 0;
                var missingFiles = new HashSet<string>();
                void TryAddImage(string file, bool imageSpecialBundle = false)
                {
                    if (file == null)
                        return;
                    var lFile = file.ToLowerInvariant();
                    if (!File.Exists(file))
                    {
                        if (!imageSpecialBundle || !lFile.StartsWith("rs_") || lFile.Contains('\\'))
                            missingFiles.Add(lFile);
                        return;
                    }
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
                    var lFile = file.ToLowerInvariant();
                    if (!File.Exists(file))
                    {
                        if (!lFile.StartsWith("rs_") || lFile.Contains('\\'))
                            missingFiles.Add(lFile);
                        return;
                    }
                    var d = assetPaths.GetOrCreate(lFile);
                    if (d.ContainsKey(asset))
                        return;
                    if (!packagedFiles.ContainsKey(lFile))
                        packagedFiles[lFile] = ($"{ind}-{Path.GetFileName(file)}{(lFile.EndsWith(".bundle") ? "" : ".bundle")}", File.ReadAllBytes(file));
                    d[asset] = $"RS_SHARED/{tick}-{simpleName}/{ind++}-{asset}";
                }
                foreach (var s in EquipmentControls)
                {
                    TryAddImage(s.data.Icon,true);
                    if (s.data is SkinData skinD)
                    {
                        if (skinD.BabyMesh != null)
                            TryAddBundle(skinD.BabyMesh.Value.Item1, skinD.BabyMesh.Value.Item2);
                        if (skinD.TeenMesh != null)
                            TryAddBundle(skinD.TeenMesh.Value.Item1, skinD.TeenMesh.Value.Item2);
                        if (skinD.AdultMesh != null)
                            TryAddBundle(skinD.AdultMesh.Value.Item1, skinD.AdultMesh.Value.Item2);
                        if (skinD.TitanMesh != null)
                            TryAddBundle(skinD.TitanMesh.Value.Item1, skinD.TitanMesh.Value.Item2);
                        foreach (var ms in new[] { skinD.Materials, skinD.HWMaterials })
                            if (ms != null)
                                foreach (var m in ms)
                                    if (Constants.TextureProperties.ContainsKey(m.Property))
                                        TryAddImage(m.Value);
                    }
                    else if (s.data is SaddleData saddleD)
                    {
                        TryAddBundle(saddleD.Mesh.Item1,saddleD.Mesh.Item2);
                        TryAddImage(saddleD.Texture,true);
                    }
                }
                if (missingFiles.Count > 0 && MessageBox.Show(this, $"The exporter was unable to find one or more files:\n • {string.Join("\n • " ,missingFiles)}\n\nWould you like to export the pack anyway?", "Missing File(s)", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
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
                foreach (var s in EquipmentControls)
                {
                    var data = s.data is SkinData ? (SimpleResourceReplacer.CustomDragonEquipment)new SimpleResourceReplacer.CustomSkin() : s.data is SaddleData ? new SimpleResourceReplacer.CustomSaddle() : null;
                    data.Name = s.data.Name;
                    data.ItemID = s.data.Id;
                    data.SkinIcon = imagePathsSpecial.TryGetValue(s.data.Icon.ToLowerInvariant(), out var v) ? v : s.data.Icon;
                    data.RequiredAge = s.data.RequiredAge;
                    var type = s.data.Type;
                    if (s.data.TypePreset != null)
                        data.PetType = Constants.TypePresets[s.data.TypePreset].Type;
                    else
                        data.PetType = s.data.Type;
                    var ext = "something";
                    if (s.data is SkinData skinD && data is SimpleResourceReplacer.CustomSkin skinC)
                    {
                        ext = "skin";
                        if (s.data.TypePreset != null)
                            skinC.TargetRenderers = Constants.TypePresets[s.data.TypePreset].Renderers;
                        else
                            skinC.TargetRenderers = skinD.Renderers.ToArray();
                        if (skinD.BabyMesh != null || skinD.TeenMesh != null || skinD.AdultMesh != null || skinD.TitanMesh != null)
                        {
                            skinC.Mesh = new SimpleResourceReplacer.MeshOverrides();
                            if (skinD.BabyMesh != null)
                                skinC.Mesh.Baby = assetPaths.TryGetValue(skinD.BabyMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[skinD.BabyMesh.Value.Item2] : $"{skinD.BabyMesh.Value.Item1}/{skinD.BabyMesh.Value.Item2}";
                            if (skinD.TeenMesh != null)
                                skinC.Mesh.Baby = assetPaths.TryGetValue(skinD.TeenMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[skinD.TeenMesh.Value.Item2] : $"{skinD.TeenMesh.Value.Item1}/{skinD.TeenMesh.Value.Item2}";
                            if (skinD.AdultMesh != null)
                                skinC.Mesh.Adult = assetPaths.TryGetValue(skinD.AdultMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[skinD.AdultMesh.Value.Item2] : $"{skinD.AdultMesh.Value.Item1}/{skinD.AdultMesh.Value.Item2}";
                            if (skinD.TitanMesh != null)
                                skinC.Mesh.Titan = assetPaths.TryGetValue(skinD.TitanMesh.Value.Item1.ToLowerInvariant(), out var d) ? d[skinD.TitanMesh.Value.Item2] : $"{skinD.TitanMesh.Value.Item1}/{skinD.TitanMesh.Value.Item2}";
                        }
                        skinC.BabyShaders = skinD.BabyShaders;
                        skinC.TeenShaders = skinD.TeenShaders;
                        skinC.AdultShaders = skinD.AdultShaders;
                        skinC.TitanShaders = skinD.TitanShaders;
                        SimpleResourceReplacer.MaterialProperty[] ConvertProps(IEnumerable<MaterialData> materials)
                        {
                            var props = new List<SimpleResourceReplacer.MaterialProperty>();
                            foreach (var i in materials)
                            {
                                var ss = (i.Age == Age.Baby ? skinD.BabyShaders : i.Age == Age.Teen ? skinD.TeenShaders : i.Age == Age.Adult ? skinD.AdultShaders : skinD.TitanShaders) ?? new SkinData.Shaders();
                                if (i.Part == Part.Eyes || i.Part == Part.Body || i.Part == Part.Extra || (i.Part == Part.Both && ss.Body == ss.Eyes) || (i.Part == Part.All && ss.Body == ss.Eyes && ss.Body == ss.Extra))
                                    props.Add(new SimpleResourceReplacer.MaterialProperty()
                                    {
                                        Target = i.Age + (i.Part == Part.Both ? "" : i.Part.ToString()),
                                        Property = i.GetRealProperty((i.Part == Part.Eyes ? ss.Eyes : i.Part == Part.Extra ? ss.Extra : ss.Body) == ShaderTypes.Default),
                                        Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                    });
                                else if (i.Part == Part.Both)
                                {
                                    props.Add(new SimpleResourceReplacer.MaterialProperty()
                                    {
                                        Target = i.Age + Part.Body.ToString(),
                                        Property = i.GetRealProperty(ss.Body == ShaderTypes.Default),
                                        Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                    });
                                    props.Add(new SimpleResourceReplacer.MaterialProperty()
                                    {
                                        Target = i.Age + Part.Eyes.ToString(),
                                        Property = i.GetRealProperty(ss.Eyes == ShaderTypes.Default),
                                        Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                    });
                                }
                                else
                                {
                                    if (ss.Body == ss.Eyes)
                                        props.Add(new SimpleResourceReplacer.MaterialProperty()
                                        {
                                            Target = i.Age.ToString(),
                                            Property = i.GetRealProperty(ss.Body == ShaderTypes.Default),
                                            Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                        });
                                    else
                                    {
                                        props.Add(new SimpleResourceReplacer.MaterialProperty()
                                        {
                                            Target = i.Age + Part.Body.ToString(),
                                            Property = i.GetRealProperty(ss.Body == ShaderTypes.Default),
                                            Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                        });
                                        props.Add(new SimpleResourceReplacer.MaterialProperty()
                                        {
                                            Target = i.Age + Part.Eyes.ToString(),
                                            Property = i.GetRealProperty(ss.Eyes == ShaderTypes.Default),
                                            Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                        });
                                    }
                                    props.Add(new SimpleResourceReplacer.MaterialProperty()
                                    {
                                        Target = i.Age + Part.Extra.ToString(),
                                        Property = i.GetRealProperty(ss.Extra == ShaderTypes.Default),
                                        Value = Constants.TextureProperties.ContainsKey(i.Property) && imagePaths.TryGetValue(i.Value.ToLowerInvariant(), out v) ? v : i.Value
                                    });
                                }
                            }
                            return props.ToArray();
                        }
                        skinC.MaterialData = ConvertProps(skinD.Materials);
                        if (skinD.HWMaterials != null)
                            skinC.HWMaterialData = ConvertProps(skinD.HWMaterials);
                    }
                    else if (s.data is SaddleData saddleD && data is SimpleResourceReplacer.CustomSaddle saddleC)
                    {
                        ext = "saddle";
                        saddleC.Mesh = (saddleC.CustomMesh = assetPaths.TryGetValue(saddleD.Mesh.Item1.ToLowerInvariant(), out var d)) ? d[saddleD.Mesh.Item2] : $"{saddleD.Mesh.Item1}/{saddleD.Mesh.Item2}";
                        saddleC.Texture = imagePathsSpecial.TryGetValue(saddleD.Texture.ToLowerInvariant(), out v) ? v : saddleD.Texture;
                    }
                    packagedFiles[$"{ind}-{s.data.Name}.{ext}"] = ($"{ind++}-{s.data.Name}.{ext}", data.JsonSerialize());
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

        void ValidateCreatorID(object sender, EventArgs e)
        {
            if (sender is NumericUpDown num && (int)num.Value == 0)
                num.Value = 1;
        }
    }

    [Serializable]
    public abstract class EquipmentData
    {
        public string Name = "";
        public int Id = ( Main.LastCreatorID,0).JoinIDs();
        public string Icon = "";
        public int Type;
        public string TypePreset;
        [OptionalField]
        public string RequiredAge = "TEEN";
        public virtual void Save(Main main)
        {
            Name = main.NameTextbox.Text;
            Id = ((int)main.CIDInput.Value, (int)main.IIDInput.Value).JoinIDs();
            Main.LastCreatorID = (int)main.CIDInput.Value;
            Icon = main.IconTextbox.Text;
            RequiredAge = main.RequiredAgeCombobox.GetSelected();
            if (main.TypeSelector.SelectedIndex == 0)
            {
                TypePreset = null;
                Type = (int)main.TypeInput.Value;
            }
            else
            {
                TypePreset = main.TypeSelector.GetSelected();
                Type = 0;
            }
        }
        public virtual void Select(Main main)
        {
            main.NameTextbox.Text = Name;
            (main.CIDInput.Value, main.IIDInput.Value) = Id.SplitIDs();
            main.IconTextbox.Text = Icon;
            main.RequiredAgeCombobox.SetSelected(RequiredAge ?? Age.Teen.ToString().ToUpperInvariant());
            if (TypePreset == null)
            {
                main.TypeSelector.SelectedIndex = 0;
                main.TypeInput.Value = Type;
            }
            else
                main.TypeSelector.SetSelected(TypePreset);
        }
    }

    [Serializable]
    public class SaddleData : EquipmentData
    {
        public (string, string) Mesh;
        public string Texture;
        public override void Save(Main main)
        {
            base.Save(main);
            Mesh = (main.SaddleBundleTextbox.Text, main.SaddleAssetTextbox.Text);
            Texture = main.SaddleTextureTextbox.Text;
        }
        public override void Select(Main main)
        {
            base.Select(main);
            main.SaddlePanel.Visible = true;
            (main.SaddleBundleTextbox.Text, main.SaddleAssetTextbox.Text) = Mesh;
            main.SaddleTextureTextbox.Text = Texture;
        }
    }

    [Serializable]
    public class SkinData : EquipmentData
    {
        public List<string> Renderers = new List<string>();
        public (string, string)? BabyMesh;
        public (string, string)? TeenMesh;
        public (string, string)? AdultMesh;
        public (string, string)? TitanMesh;
        public List<MaterialData> Materials = new List<MaterialData>();
        public List<MaterialData> HWMaterials;
        [OptionalField]
        public Shaders BabyShaders = new Shaders();
        [OptionalField]
        public Shaders TeenShaders = new Shaders();
        [OptionalField]
        public Shaders AdultShaders = new Shaders();
        [OptionalField]
        public Shaders TitanShaders = new Shaders();
        [Serializable]
        public class Shaders
        {
            [OptionalField]
            public ShaderTypes Body = ShaderTypes.Default;
            [OptionalField]
            public ShaderTypes Eyes = ShaderTypes.Default;
            [OptionalField]
            public ShaderTypes Extra = ShaderTypes.Extra;
            public static implicit operator SimpleResourceReplacer.CustomSkin.Shaders(Shaders s) =>
                s == null || (s.Body == ShaderTypes.Default && s.Eyes == ShaderTypes.Default && s.Extra == ShaderTypes.Extra)
                ? null
                : new SimpleResourceReplacer.CustomSkin.Shaders()
                {
                    Body = s.Body == ShaderTypes.Default ? null : s.Body.ToString(),
                    Eyes = s.Eyes == ShaderTypes.Default ? null : s.Eyes.ToString(),
                    Extra = s.Extra == ShaderTypes.Extra ? null : s.Extra.ToString()
                };
        }
        public override void Save(Main main)
        {
            base.Save(main);
            if (main.TypeSelector.SelectedIndex == 0)
            {
                Renderers.Clear();
                foreach (var i in main.RendererList.Items)
                    Renderers.Add(i.ToString());
            }
            else
                Renderers.Clear();
            BabyMesh = string.IsNullOrWhiteSpace(main.BabyBundleTextbox.Text) ? null : ((string, string)?)(main.BabyBundleTextbox.Text, main.BabyAssetTextbox.Text);
            TeenMesh = string.IsNullOrWhiteSpace(main.TeenBundleTextbox.Text) ? null : ((string, string)?)(main.TeenBundleTextbox.Text, main.TeenAssetTextbox.Text);
            AdultMesh = string.IsNullOrWhiteSpace(main.AdultBundleTextbox.Text) ? null : ((string, string)?)(main.AdultBundleTextbox.Text, main.AdultAssetTextbox.Text);
            TitanMesh = string.IsNullOrWhiteSpace(main.TitanBundleTextbox.Text) ? null : ((string, string)?)(main.TitanBundleTextbox.Text, main.TitanAssetTextbox.Text);
            Shaders GetShaders(ComboBox body, ComboBox eyes, ComboBox extra) =>
                body.GetSelected() == ShaderTypes.Default.ToString()
                && eyes.GetSelected() == ShaderTypes.Default.ToString()
                && extra.GetSelected() == ShaderTypes.Extra.ToString()
                ? null
                : new Shaders()
                {
                    Body = (ShaderTypes)Enum.Parse(typeof(ShaderTypes), body.GetSelected()),
                    Eyes = (ShaderTypes)Enum.Parse(typeof(ShaderTypes), eyes.GetSelected()),
                    Extra = (ShaderTypes)Enum.Parse(typeof(ShaderTypes), extra.GetSelected())
                };
            BabyShaders = GetShaders(main.BabyBodyShaderCombobox, main.BabyEyesShaderCombobox, main.BabyExtraShaderCombobox);
            TeenShaders = GetShaders(main.TeenBodyShaderCombobox, main.TeenEyesShaderCombobox, main.TeenExtraShaderCombobox);
            AdultShaders = GetShaders(main.AdultBodyShaderCombobox, main.AdultEyesShaderCombobox, main.AdultExtraShaderCombobox);
            TitanShaders = GetShaders(main.TitanBodyShaderCombobox, main.TitanEyesShaderCombobox, main.TitanExtraShaderCombobox);
            Materials.Clear();
            foreach (var p in main.MaterialProperties)
            {
                Materials.Add(p.data);
                p.Save();
            }
            if (main.HWCheckbox.Checked)
            {
                HWMaterials = new List<MaterialData>();
                foreach (var p in main.HWMaterialProperties)
                {
                    HWMaterials.Add(p.data);
                    p.Save();
                }
            }
            else
                HWMaterials = null;
        }
        public override void Select(Main main)
        {
            base.Select(main);
            main.SkinPanel.Visible = true;
            if (TypePreset == null)
            {
                main.RendererList.Items.Clear();
                foreach (var i in Renderers)
                    main.RendererList.Items.Add(i);
            }
            (main.BabyBundleTextbox.Text, main.BabyAssetTextbox.Text) = BabyMesh ?? ("", "");
            (main.TeenBundleTextbox.Text, main.TeenAssetTextbox.Text) = TeenMesh ?? ("", "");
            (main.AdultBundleTextbox.Text, main.AdultAssetTextbox.Text) = AdultMesh ?? ("", "");
            (main.TitanBundleTextbox.Text, main.TitanAssetTextbox.Text) = TitanMesh ?? ("", "");
            void SetShaders(Shaders shaders, ComboBox body, ComboBox eyes, ComboBox extra)
            {
                body.SetSelected((shaders?.Body ?? ShaderTypes.Default).ToString());
                eyes.SetSelected((shaders?.Eyes ?? ShaderTypes.Default).ToString());
                extra.SetSelected((shaders?.Extra ?? ShaderTypes.Default).ToString());
            }
            SetShaders(BabyShaders, main.BabyBodyShaderCombobox, main.BabyEyesShaderCombobox, main.BabyExtraShaderCombobox);
            SetShaders(TeenShaders, main.TeenBodyShaderCombobox, main.TeenEyesShaderCombobox, main.TeenExtraShaderCombobox);
            SetShaders(AdultShaders, main.AdultBodyShaderCombobox, main.AdultEyesShaderCombobox, main.AdultExtraShaderCombobox);
            SetShaders(TitanShaders, main.TitanBodyShaderCombobox, main.TitanEyesShaderCombobox, main.TitanExtraShaderCombobox);
            while (main.MaterialProperties.Count > 0)
            {
                main.MaterialProperties[0].Dispose();
                main.MaterialProperties.RemoveAt(0);
            }
            foreach (var m in Materials)
                main.MaterialProperties.Add(
                    Constants.TextureProperties.ContainsKey(m.Property) ? new TextureMaterialProperty(main, main.MaterialsLayout, m)
                    : Constants.ColorProperties.ContainsKey(m.Property) ? (MaterialProperty)new ColorMaterialProperty(main, main.MaterialsLayout, m)
                    : new FloatMaterialProperty(main, main.MaterialsLayout, m));
            while (main.HWMaterialProperties.Count > 0)
            {
                main.HWMaterialProperties[0].Dispose();
                main.HWMaterialProperties.RemoveAt(0);
            }
            if (main.HWCheckbox.Checked = HWMaterials != null)
                foreach (var m in HWMaterials)
                    main.HWMaterialProperties.Add(
                        Constants.TextureProperties.ContainsKey(m.Property) ? new TextureMaterialProperty(main, main.HWMaterialsLayout, m)
                        : Constants.ColorProperties.ContainsKey(m.Property) ? (MaterialProperty)new ColorMaterialProperty(main, main.HWMaterialsLayout, m)
                        : new FloatMaterialProperty(main, main.HWMaterialsLayout, m));
        }
    }

    public class EquipmentControl : Panel
    {
        public PictureBox img;
        public Label lbl;
        public readonly EquipmentData data;
        public EquipmentControl(Main main, Control parent, EquipmentData equipment)
        {
            data = equipment;
            Parent = parent;
            img = new PictureBox();
            img.Parent = this;
            lbl = new Label();
            lbl.Parent = this;

            Anchor = AnchorStyles.Top;
            AutoSize = true;
            BackColor = SystemColors.Control;
            Name = "EquipmentButton";
            Click += (x, y) => main.TrySelectEquipment(this);

            img.Location = new Point(3, 3);
            img.Size = new Size(100, 100);
            img.Name = "EquipmentIcon";
            img.SizeMode = PictureBoxSizeMode.Zoom;
            img.Click += (x, y) => main.TrySelectEquipment(this);

            lbl.AutoSize = true;
            lbl.Location = new Point(3, 106);
            lbl.Margin = new Padding(3, 0, 3, 3);
            lbl.MinimumSize = lbl.MaximumSize = new Size(100, 0);
            lbl.Name = "EquipmentName";
            lbl.Click += (x, y) => main.TrySelectEquipment(this);

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

        public string GetRealProperty(bool main) => Constants.TextureProperties.TryGetValue(Property, out var n) ? main ? n.main : n.secondary : Constants.ColorProperties.TryGetValue(Property, out var n2) ? main ? n2.main : n2.secondary : Constants.FloatProperties.TryGetValue(Property, out var n3) ? main ? n3.main : n3.secondary : Property;
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
        Body,
        Extra,
        All,
        Eyes = Eye
    }

    public enum ShaderTypes
    {
        Default,
        Eyes,
        Extra
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
            TargetPart.SetContents(Enum.GetNames(typeof(Part)).Where(x => x != "Eye"), data.Part.ToString());
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
            data.Age = (Age)Enum.Parse(typeof(Age), TargetAge.GetSelected());
            data.Part = (Part)Enum.Parse(typeof(Part), TargetPart.GetSelected());
            data.Property = Property.GetSelected();
        }
    }

    public class TextureMaterialProperty : MaterialProperty
    {
        public TextBox Input;
        public TextureMaterialProperty(Main main, Control parent, MaterialData material) : base(main,parent,material,Constants.TextureProperties.Select(x => x.Key + (x.Value.secondary == null ? " (Default Only)" : x.Value.main == null ? " (Not Default)" : "")))
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
        public ColorMaterialProperty(Main main, Control parent, MaterialData material) : base(main, parent, material, Constants.ColorProperties.Select(x => x.Key + (x.Value.secondary == null ? " (Default Only)" : x.Value.main == null ? " (Not Default)" : "")))
        {
            Btn = new Button();
            Btn.Parent = this;
            var Label = new Label();
            Label.Parent = this;

            Label.AutoSize = true;
            Label.Location = new Point(3, 60);
            Label.Text = "Color:";
            Label.Name = "ColorLabel";

            Btn.BackColor = material.Value.HexToColor();
            Btn.Location = new Point(58, 57);
            Btn.Size = new Size(23, 23);
            Btn.UseVisualStyleBackColor = false;
            Btn.Name = "ColorButton";
            Btn.BackColorChanged += main.ControlChanged;
            Btn.Click += main.PickColor;

            lastProp = Property.GetSelected();
            Property.SelectedIndexChanged += (x, y) =>
            {
                if (Equals(Btn.BackColor, Constants.ColorProperties[lastProp].value))
                    Btn.BackColor = Constants.ColorProperties[Property.GetSelected()].value;
                lastProp = Property.GetSelected();
            };
        }
        string lastProp;

        static bool Equals(Color a, Color b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;

        public override void Save()
        {
            base.Save();
            data.Value = Btn.BackColor.ToHex();
        }
    }

    public class FloatMaterialProperty : MaterialProperty
    {
        public TextBox Input;
        public FloatMaterialProperty(Main main, Control parent, MaterialData material) : base(main, parent, material, Constants.FloatProperties.Select(x => x.Key + (x.Value.secondary == null ? " (Default Only)" : x.Value.main == null ? " (Not Default)" : "")))
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

            lastProp = Property.GetSelected();
            Property.SelectedIndexChanged += (x, y) =>
            {
                if (Input.Text == Constants.FloatProperties[lastProp].value.ToString())
                    Input.Text = Constants.FloatProperties[Property.GetSelected()].value.ToString();
                lastProp = Property.GetSelected();
            };
        }
        string lastProp;

        public override void Save()
        {
            base.Save();
            data.Value = Input.Text;
        }
    }

    public class HelpButton : Button
    {
        protected override void OnClick(EventArgs e)
        {
            MessageBox.Show(FindForm(), Tag.ToString(), "Help", MessageBoxButtons.OK, MessageBoxIcon.Question);
            base.OnClick(e);
        }
    }

    public static class Constants
    {
        public static SortedDictionary<string, (string main, string secondary)> TextureProperties = new SortedDictionary<string, (string, string)>()
        {
            {"Normal Map",("_BumpMap",null) },
            {"Colour Mask",("_ColorMask",null) },
            {"Decal Map",("_DecalMap",null) },
            {"Main Texture",("_DetailTex","_MainTex") },
            {"Emissive Map",("_EmissiveMap",null) },
            {"Glow Map",("_MKGlowTex",null) },
            {"Specular Map",("_SpecularMap",null) }
        };
        public static SortedDictionary<string, (string main, string secondary, Color value)> ColorProperties = new SortedDictionary<string, (string, string, Color)>()
        {
            {"Emissive Colour",("_EmissiveColor","_Emission", Color.Black) },
            {"Glow Colour",("_MKGlowColor", null, Color.White) },
            {"Glow Texture Colour",("_MKGlowTexColor", null, Color.White) },
            {"Specular Colour",("_SpecColor", "_SpecColor", Color.Gray) },
            {"Colour",(null, "_Color", Color.White) }
        };
        public static SortedDictionary<string, (string main, string secondary, float value)> FloatProperties = new SortedDictionary<string, (string, string, float)>()
        {
            {"Normal Strength",("_BumpStrength",null, 1) },
            {"Decal Opacity",("_DecalOpacity",null,0) },
            {"Glossiness",("_Glossiness","_Shininess",0.5f) },
            {"Glow Power",("_MKGlowPower",null,0) },
            {"Glow Texture Strength",("_MKGlowTexStrength",null,0) }
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
            { "Crimson Howler", (128, new[] { "PfDWCrimsonHowler" })}
        };
        public static Dictionary<string, string> PresetNameCorrects = new Dictionary<string, string>
        {
            { "Crimson Holwer", "Crimson Howler" }
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
                else if (p.Split(' ')[0] == selected)
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
                else if ((p as string).Split(' ')[0] == selected)
                    flag = true;
                else
                    ind++;
            }
            if (ind == combo.Items.Count)
                ind = 0;
            combo.SelectedIndex = ind;
        }
        public static string GetSelected(this ComboBox combo) => (combo.Items[combo.SelectedIndex] as string).Split(' ')[0];
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
        public static int JoinIDs(this (int creator, int item) ids) => ids.creator * 100000 + ids.item * (ids.creator < 0 ? -1 : 1);
        public static (int creator, int item) SplitIDs(this int id)
        {
            var c = id / 100000;
            if (c * 100000 == id)
                return (c, 0);
            return (c, Math.Abs(id - c * 100000));
        }
        public static void TrySetFile(this FileDialog dialog, string file)
        {
            if (file == null)
                return;
            dialog.FileName = Path.GetFileName(file);
            if (!string.IsNullOrWhiteSpace(file) && Directory.Exists(Path.GetDirectoryName(file)))
                dialog.InitialDirectory = Path.GetDirectoryName(file);
        }
    }

    public static class ColorConvert
    {
        public static void ToHSL(this Color c, out int hue, out int saturation, out int luminosity) => ToHSL(c.R, c.G, c.B, out hue, out saturation, out luminosity);
        public static void ToHSL(byte R, byte G, byte B, out int hue, out int saturation, out int luminosity)
        {
            var max = Math.Max(Math.Max(R, G), B);
            var min = Math.Min(Math.Min(R, G), B);
            luminosity = max;
            if (min == max)
            {
                hue = 0;
                saturation = 0;
                return;
            }
            saturation = (max - min) * 255 / max;
            if (R == max)
            {
                if (G >= B)
                    hue = (G - min) * 60 / (max - min);
                else
                    hue = 360 - (B - min) * 60 / (max - min);
            }
            else if (G == max)
            {
                if (B >= R)
                    hue = 120 + (B - min) * 60 / (max - min);
                else
                    hue = 120 - (R - min) * 60 / (max - min);
            }
            else
            {
                if (R >= G)
                    hue = 240 + (R - min) * 60 / (max - min);
                else
                    hue = 240 - (G - min) * 60 / (max - min);
            }
        }
        public static Color FromHSL(int hue, int saturation, int luminosity)
        {
            FromHSL(hue, saturation, luminosity, out var R, out var G, out var B);
            return Color.FromArgb(R, G, B);
        }
        public static void FromHSL(int hue, int saturation, int luminosity, out byte R, out byte G, out byte B)
        {
            hue %= 360;
            if (hue < 0)
                hue += 360;
            if (saturation > 255)
                saturation = 255;
            else if (saturation < 0)
                saturation = 0;
            if (luminosity > 255)
                luminosity = 255;
            else if (luminosity < 0)
                luminosity = 0;
            var max = (byte)luminosity;
            if (saturation == 0)
            {
                R = G = B = max;
                return;
            }
            var min = (byte)(max - (saturation * max / 255));
            if (hue <= 60)
            {
                B = min;
                R = max;
                G = (byte)(min + hue * (max - min) / 60);
            }
            else if (hue <= 120)
            {
                B = min;
                G = max;
                R = (byte)(min + (120 - hue) * (max - min) / 60);
            }
            else if (hue <= 180)
            {
                R = min;
                G = max;
                B = (byte)(min + (hue - 120) * (max - min) / 60);
            }
            else if (hue <= 240)
            {
                R = min;
                B = max;
                G = (byte)(min + (240 - hue) * (max - min) / 60);
            }
            else if (hue <= 300)
            {
                G = min;
                B = max;
                R = (byte)(min + (hue - 240) * (max - min) / 60);
            }
            else
            {
                G = min;
                R = max;
                B = (byte)(min + (360 - hue) * (max - min) / 60);
            }
        }
        public static Color HexToColor(this string str) => uint.TryParse(str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var c) ? Color.FromArgb((c | (str.Length <= 6 ? 0xFF000000 : 0)).AsInt()) : Color.Black;
        public static string ToHex(this Color c) => $"{c.ToArgb().AsUInt():X8}";
    }

    [Serializable]
    public class Settings
    {
        static FileStream settings = File.Open("SkinPackMaker.settings.json", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Settings));
        public readonly static Settings Instance;
        public int LastCreatorID = 1;
        static Settings()
        {
            try
            {
                Instance = (Settings)serializer.ReadObject(settings);
            } catch
            {
                Instance = new Settings();
            }
        }
        public static void Save()
        {
            settings.Seek(0, SeekOrigin.Begin);
            serializer.WriteObject(settings, Instance);
        }
    }
}
