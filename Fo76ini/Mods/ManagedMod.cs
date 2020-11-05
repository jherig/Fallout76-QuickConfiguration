﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fo76ini.Mods
{
    public class ManagedMod
    {
        /// <summary>
        /// Contains information about the current or desired deployment state.
        /// </summary>
        public class DiskState
        {
            /// <summary>
            /// How a mod should be deployed.
            /// Loose       - Copy files over without packing
            /// BundledBA2  - Bundle it with other mods in one package
            /// SeparateBA2 - Pack it as a separate *.ba2 archive
            /// </summary>
            public enum DeploymentMethod
            {
                Loose,
                BundledBA2,
                SeparateBA2
            }

            /// <summary>
            /// Archive format
            /// Auto     - Automatically detect
            /// General  - Use "Archive2.Format.General"
            /// Textures - Use "Archive2.Format.DDS"
            /// 
            /// (Does only apply to DeploymentMethod.SeparateBA2)
            /// </summary>
            public enum ArchiveFormat
            {
                Auto,
                General,
                Textures
            }

            /// <summary>
            /// Archive compression
            /// Auto         - Automatically detect
            /// Compressed   - Use "Archive2.Compression.Default"
            /// Uncompressed - Use "Archive2.Compression.None"
            /// 
            /// (Does only apply to DeploymentMethod.SeparateBA2)
            /// </summary>
            public enum ArchiveCompression
            {
                Auto,
                Compressed,
                Uncompressed
            }

            public bool Enabled = false;

            public DeploymentMethod Method = DeploymentMethod.BundledBA2;

            /// <summary>
            /// The folder where to copy loose files to.
            /// </summary>
            public String RootFolder;

            /// <summary>
            /// Folder name (not path). This folder stores the mod's files.
            /// Example: @"2f2d3b3b-b21b-4ec2-b555-c8806a801b16"
            /// Path:    @"C:\Program Files (x86)\Steam\steamapps\common\Fallout 76\Mods\2f2d3b3b-b21b-4ec2-b555-c8806a801b16\"
            /// </summary>
            public String ManagedFolderName;

            /* Does only apply for Loose */
            public List<String> LooseFiles = new List<String>();

            /* Does only apply for SeparateBA2 */
            public ArchiveCompression Compression = ArchiveCompression.Auto;
            private String archiveName = "untitled.ba2";
            public ArchiveFormat Format = ArchiveFormat.Auto;
            public bool Frozen = false;

            /// <summary>
            /// Creates an empty object.
            /// </summary>
            public DiskState() { }

            public DiskState(Guid uuid)
            {
                this.ManagedFolderName = uuid.ToString();
            }

            /// <summary>
            /// Creates a deep copy of 'state'.
            /// </summary>
            /// <param name="state">The object it makes a copy of.</param>
            public DiskState(DiskState state)
            {
                this.LooseFiles = new List<String>(state.LooseFiles);
                this.ManagedFolderName = state.ManagedFolderName;
                this.Enabled = state.Enabled;
                this.Method = state.Method;
                this.RootFolder = state.RootFolder;
                this.Compression = state.Compression;
                this.Format = state.Format;
                this.archiveName = state.archiveName;
                this.Frozen = state.Frozen;
            }

            public DiskState CreateDeepCopy()
            {
                return new DiskState(this);
            }

            /// <summary>
            /// Name of the archive in @"Fallout 76\Data".
            /// (Does only apply to DeploymentMethod.SeparateBA2)
            /// </summary>
            public String ArchiveName
            {
                get { return this.archiveName; }
                set
                {
                    if (value.Trim().Length < 0)
                        return;
                    this.archiveName = Utils.GetValidFileName(value, ".ba2");
                }
            }

            /// <summary>
            /// Get the name of the deployment method.
            /// </summary>
            /// <returns></returns>
            public String GetMethodName()
            {
                return Enum.GetName(typeof(DeploymentMethod), (int)this.Method);
            }

            /// <summary>
            /// Get the name of the archive format.
            /// </summary>
            /// <returns></returns>
            public String GetFormatName()
            {
                return Enum.GetName(typeof(ArchiveFormat), (int)this.Format);
            }

            /// <summary>
            /// Get the name of the archive compression.
            /// </summary>
            /// <returns></returns>
            public String GetCompressionName()
            {
                return Enum.GetName(typeof(ArchiveCompression), (int)this.Compression);
            }

            /// <summary>
            /// Get the path to where the mod's files are.
            /// </summary>
            /// <returns>Example: @"C:\Program Files (x86)\Steam\steamapps\common\Fallout 76\Mods\2f2d3b3b-b21b-4ec2-b555-c8806a801b16\"</returns>
            public String GetManagedPath()
            {
                return Path.Combine(Shared.GamePath, "Mods", this.ManagedFolderName);
            }

            /// <summary>
            /// Get the path to where the mod's frozen archive is stored.
            /// </summary>
            /// <returns>Example: @"C:\Program Files (x86)\Steam\steamapps\common\Fallout 76\Mods\2f2d3b3b-b21b-4ec2-b555-c8806a801b16\frozen.ba2"</returns>
            public String GetFrozenArchivePath()
            {
                return Path.Combine(GetManagedPath(), "frozen.ba2");
            }

            /// <summary>
            /// Clear the list 'LooseFiles'.
            /// </summary>
            public void ClearFiles()
            {
                this.LooseFiles.Clear();
            }

            /// <summary>
            /// Adds a relative file path to the list 'LooseFiles'.
            /// </summary>
            /// <param name="path">Relative file path</param>
            public void AddFile(String path)
            {
                this.LooseFiles.Add(path);
            }

            public XElement Serialize(XElement parent)
            {
                parent.Add(
                    new XAttribute("enabled", this.Enabled),
                    new XElement("DeploymentMethod", this.GetMethodName()),
                    new XElement("ManagedFolder", this.ManagedFolderName)
                );

                if (this.Method == DeploymentMethod.Loose)
                {
                    parent.Add(new XElement("Destination", this.RootFolder));

                    if (this.Enabled)
                    {
                        XElement files = new XElement("Files");
                        foreach (String filePath in this.LooseFiles)
                            files.Add(new XElement("File", new XAttribute("path", filePath)));
                        parent.Add(files);
                    }
                }

                if (this.Method == DeploymentMethod.SeparateBA2)
                {
                    XElement archive = new XElement("Archive");
                    archive.Add(
                        new XAttribute("frozen", this.Frozen),
                        new XElement("Format", this.GetFormatName()),
                        new XElement("Compression", this.GetCompressionName()),
                        new XElement("ArchiveName", this.ArchiveName)
                    );
                    parent.Add(archive);
                }

                return parent;
            }

            public static DiskState Deserialize(XElement xmlDiskState)
            {
                DiskState diskState = new DiskState();
                diskState.ManagedFolderName = xmlDiskState.Element("ManagedFolder").Value;

                try
                {
                    diskState.Enabled = Convert.ToBoolean(xmlDiskState.Attribute("enabled").Value);
                }
                catch (FormatException ex)
                {
                    throw new InvalidDataException($"Invalid 'enabled' value: {xmlDiskState.Attribute("enabled").Value}");
                }

                switch (xmlDiskState.Element("DeploymentMethod").Value)
                {
                    case "Loose":
                        diskState.Method = DeploymentMethod.Loose;
                        break;
                    case "BundledBA2":
                        diskState.Method = DeploymentMethod.BundledBA2;
                        break;
                    case "SeparateBA2":
                        diskState.Method = DeploymentMethod.SeparateBA2;
                        break;
                    default:
                        throw new InvalidDataException($"Invalid mod deployment method: {xmlDiskState.Element("DeploymentMethod").Value}");
                }

                if (diskState.Method == DeploymentMethod.Loose)
                {
                    diskState.RootFolder = xmlDiskState.Element("Destination").Value;

                    XElement xmlFiles = xmlDiskState.Element("Files");
                    if (diskState.Enabled && xmlFiles != null)
                        foreach (XElement xmlFile in xmlFiles.Descendants("File"))
                            if (xmlFile.Attribute("path") != null)
                                diskState.AddFile(xmlFile.Attribute("path").Value);
                }

                if (diskState.Method == DeploymentMethod.SeparateBA2)
                {
                    XElement xmlArchive = xmlDiskState.Element("Archive");

                    diskState.ArchiveName = xmlArchive.Element("ArchiveName").Value;

                    try
                    {
                        diskState.Frozen = Convert.ToBoolean(xmlArchive.Attribute("frozen").Value);
                    }
                    catch (FormatException ex)
                    {
                        throw new InvalidDataException($"Invalid 'frozen' value: {xmlArchive.Attribute("frozen").Value}");
                    }

                    switch (xmlArchive.Element("Format").Value)
                    {
                        case "General":
                            diskState.Format = ArchiveFormat.General;
                            break;
                        case "Textures":
                            diskState.Format = ArchiveFormat.Textures;
                            break;
                        case "Auto":
                        default:
                            diskState.Format = ArchiveFormat.Auto;
                            break;
                    }

                    switch (xmlArchive.Element("Compression").Value)
                    {
                        case "Compressed":
                            diskState.Compression = ArchiveCompression.Compressed;
                            break;
                        case "Uncompressed":
                            diskState.Compression = ArchiveCompression.Uncompressed;
                            break;
                        case "Auto":
                        default:
                            diskState.Compression = ArchiveCompression.Auto;
                            break;
                    }
                }

                return diskState;
            }
        }

        public Guid Uuid;

        /// <summary>
        /// How the mod is currently deployed
        /// </summary>
        public DiskState CurrentDiskState;

        /// <summary>
        /// How the mod will be deployed on next deployment
        /// </summary>
        public DiskState PendingDiskState;

        private String title;
        public String Version = "1.0";
        private String url = "";
        public int ID = -1;

        public ManagedMod()
        {
            this.Uuid = Guid.NewGuid();
            this.CurrentDiskState = new DiskState(this.Uuid);
            this.PendingDiskState = new DiskState(this.Uuid);
        }

        /// <summary>
        /// Creates a deep copy of 'mod'.
        /// </summary>
        /// <param name="mod">The object it makes a copy of.</param>
        public ManagedMod(ManagedMod mod)
        {
            this.title = mod.title;
            this.ID = mod.ID;
            this.URL = mod.URL;
            this.Version = mod.Version;
            this.CurrentDiskState = this.CurrentDiskState.CreateDeepCopy();
            this.PendingDiskState = this.PendingDiskState.CreateDeepCopy();
        }

        /// <summary>
        /// URL to the NexusMods page of the mod.
        /// </summary>
        public String URL
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
                this.ID = NexusMods.GetIDFromURL(value);
            }
        }

        /// <summary>
        /// Returns remote info from NexusMods if available.
        /// Returns null if not available.
        /// </summary>
        public NMMod RemoteInfo
        {
            get
            {
                if (this.ID >= 0 && NexusMods.Mods.ContainsKey(this.ID))
                    return NexusMods.Mods[this.ID];
                else
                    return null;
            }
        }

        public String Title
        {
            get { return this.title; }
            set { this.title = value.Trim().Length > 0 ? value.Trim() : "Untitled"; }
        }

        public ManagedMod CreateDeepCopy()
        {
            return new ManagedMod(this);
        }

        public XElement Serialize()
        {
            XElement xmlMod = new XElement("Mod",
                new XAttribute("uuid", this.Uuid.ToString()),
                new XElement("Title", this.Title),
                new XElement("Version", this.Version)
            );

            XElement xmlNexusMods = new XElement("NexusMods",
                new XAttribute("id", this.ID),
                new XElement("URL", this.URL)
            );
            XElement xmlCurrentDiskState = this.CurrentDiskState.Serialize(new XElement("CurrentDiskState"));
            XElement xmlPendingDiskState = this.PendingDiskState.Serialize(new XElement("PendingDiskState"));

            xmlMod.Add(
                xmlNexusMods,
                xmlCurrentDiskState,
                xmlPendingDiskState
            );

            return xmlMod;
        }

        public static ManagedMod Deserialize(XElement xmlMod)
        {
            ManagedMod mod = new ManagedMod();
            mod.Uuid = new Guid(xmlMod.Attribute("uuid").Value);
            mod.Title = xmlMod.Element("Title").Value;
            mod.Version = xmlMod.Element("Version").Value;

            XElement xmlNexusMods = xmlMod.Element("NexusMods");
            mod.ID = Convert.ToInt32(xmlNexusMods.Attribute("id").Value);
            mod.URL = xmlNexusMods.Element("URL").Value;

            mod.CurrentDiskState = DiskState.Deserialize(xmlMod.Element("CurrentDiskState"));
            mod.PendingDiskState = DiskState.Deserialize(xmlMod.Element("PendingDiskState"));

            return mod;
        }
    }
}