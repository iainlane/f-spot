using System;
using System.IO;
using System.Collections;
using GPhoto2;
using Gdk;
using FSpot.Utils;
using FSpot;
using Mono.Unix.Native;

public class GPhotoCamera
{
	Context context;
	PortInfoList port_info_list;
	CameraAbilitiesList abilities_list;
	CameraList camera_list;
	CameraAbilities camera_abilities;
	Camera camera;
	PortInfo port_info;
	ArrayList files;
	
	int selected_camera__camera_list_index;
	int selected_camera__port_info_list_index;
		
	public GPhotoCamera()
	{
		context = new Context();

		port_info_list = new PortInfoList ();
		port_info_list.Load ();
			
		abilities_list = new CameraAbilitiesList ();
		abilities_list.Load (context);
			
		selected_camera__camera_list_index = -1;

		camera = null;
	}
		
	public int DetectCameras ()
	{
		camera_list = abilities_list.Detect (port_info_list, context);
		return CameraCount;
	}
		
	public int CameraCount {
		get { return camera_list.Count; }
	}
	
	public CameraList CameraList {
		get { return camera_list; }
	}
		
	public void SelectCamera (int index)
	{
		selected_camera__camera_list_index = index;

		camera_abilities = abilities_list [camera_list.GetName (selected_camera__camera_list_index)];
		port_info = port_info_list.LookupPath (camera_list.GetValue (selected_camera__camera_list_index));

		Log.Debug ("PortInfo {0}, {1}", port_info.Name, port_info.Path);

		camera = new Camera () { Abilities = camera_abilities, PortInfo = port_info };
	}
		
	public void InitializeCamera ()
	{
		if (camera == null) 
			throw new InvalidOperationException();

		camera.Init (context);

		files = new ArrayList ();
		GetFileList ();
	}
		
	private void GetFileList ()
	{
		GetFileList ("/");
	}
		
	private void GetFileList (string dir)
	{
		if (camera == null) 
			throw new InvalidOperationException ();

		//workaround for nikon dslr in ptp mode
		if (dir == "/special/")
			return;
		
		//files
		CameraList filelist = camera.ListFiles(dir, context);
		for (int i = 0; i < filelist.Count; i++) {
			files.Add(new GPhotoCameraFile(dir, filelist.GetName(i)));
		}
	
		//subdirectories
		CameraList folderlist = camera.ListFolders(dir, context);
		for (int i = 0; i < folderlist.Count; i++) {
			GetFileList(dir + folderlist.GetName(i) + "/");
		}
	}
	
	public ArrayList FileList
	{
		get {
			return files;
		}
	}
	
	public CameraFile GetFile (GPhotoCameraFile camfile)
	{
		int index = files.IndexOf (camfile);
		return GetFile (index);
	}
	
	public CameraFile GetFile (int index)
	{
		if (camera == null || files == null || index < 0 || index >= files.Count) 
			return null;

		GPhotoCameraFile selected_file = (GPhotoCameraFile)files [index];		
		if (selected_file.NormalFile == null)
		{
			selected_file.NormalFile = new CameraFile ();
			camera.GetFile (selected_file.Directory, selected_file.FileName, CameraFileType.Normal, selected_file.NormalFile, context);
		}
		
		return selected_file.NormalFile;
	}
	
	public CameraFile GetPreview (GPhotoCameraFile camfile)
	{
		int index = files.IndexOf (camfile);
		return GetPreview (index);
	}
	
	public CameraFile GetPreview (int index)
	{      
		if (camera == null || files == null || index < 0 || index >= files.Count) 
			return null;

		GPhotoCameraFile selected_file = (GPhotoCameraFile) files [index];		

		if (selected_file.PreviewFile == null) {
			try {
				selected_file.PreviewFile = new CameraFile ();
				camera.GetFile (selected_file.Directory, selected_file.FileName, CameraFileType.Preview, selected_file.PreviewFile, context);
			} catch (System.Exception e) {
				Log.Exception (e);
				selected_file.PreviewFile = null;
			}
		}
		
		return selected_file.PreviewFile;
	}
	
	public Pixbuf GetPreviewPixbuf (GPhotoCameraFile camfile)
	{
		CameraFile cfile = GetPreview (camfile);
		if (cfile != null) {
			byte[] bytedata = cfile.GetDataAndSize ();
			if (bytedata.Length > 0) {
				MemoryStream dataStream = new MemoryStream (bytedata);
				try {
					Gdk.Pixbuf temp = new Pixbuf (dataStream);
					Cms.Profile screen_profile;
					if (FSpot.ColorManagement.Profiles.TryGetValue (Preferences.Get<string> (Preferences.COLOR_MANAGEMENT_DISPLAY_PROFILE), out screen_profile)) 
						FSpot.ColorManagement.ApplyProfile (temp, screen_profile);
					return temp;
				} catch (Exception e) {
					// Actual errors with the data libgphoto gives us have been
					// observed here see b.g.o #357569. 
					Log.Information ("Error retrieving preview image");
					Log.DebugException (e);
				}
					
			}
		}
		return null;
	}
	
	public void SaveFile (int index, string filename)
	{
		if (filename == null) 
			return;
		
		//check if the directory exists
		if (!Directory.Exists (Path.GetDirectoryName (filename))) 
			throw new Exception (String.Format ("Directory \"{0}\"does not exist", filename)); //FIXME
		//gp_file_new_from_fd is broken on the directory driver
		//but using gp_file_new_from_fd doesn't move the files to memory
		if (camera_abilities.PortType != PortType.Disk) {
			GPhotoCameraFile selected_file = (GPhotoCameraFile) files [index];		
			using (var f = new CameraFile (Syscall.open (filename, OpenFlags.O_CREAT|OpenFlags.O_RDWR, FilePermissions.DEFFILEMODE))) {
				camera.GetFile (selected_file.Directory, selected_file.FileName, CameraFileType.Normal, f, context);
			}
			return;
		}
	
		using (CameraFile camfile = GetFile (index)) {
			if (camfile == null) 
				throw new Exception ("Unable to claim file"); //FIXME
			camfile.Save (filename);
		}
	}
		
	public void SaveAllFiles (string prefix, int start_number)
	{		
		for(int index = 0; index < files.Count; index++) {
			GPhotoCameraFile curFile = (GPhotoCameraFile) files [index];
			string extension = Path.GetExtension (curFile.FileName).ToLower ();
			SaveFile (index, prefix + Convert.ToString (start_number + index) + extension);
		}
	}
	
	~GPhotoCamera ()
	{
		ReleaseGPhotoResources ();
	}
	
	public void ReleaseGPhotoResources ()
	{
		//Dispose of GPhoto stuff to free up resources, in reverse order of course
		if (files != null)
			foreach (GPhotoCameraFile curcamfile in files)
				curcamfile.ReleaseGPhotoResources ();

		if (camera != null) 
			camera.Dispose ();

		// FIXME check to make sure we don't need to dispose of these
		// things explicitly.
		/*
		camera_list.Dispose ();
		abilities_list.Dispose ();
		port_info_list.Dispose ();
		*/

		context.Dispose ();
	}
}

	
public class GPhotoCameraFile : IComparable
{
	string directory;
	string filename;
	CameraFile normal;
	CameraFile preview;
	
	public GPhotoCameraFile (string dir, string name)
	{
		directory = dir;
		filename = name;
		normal = null;
		preview = null;
	}
		
	public string Directory {
		get {
			return directory;
		}
	}
		
	public string FileName {
		get {
			return filename;
		}
	}
	
	public CameraFile NormalFile {
		get {
			return normal;
		}
		set {
			normal = value;
		}
	}
	
	public CameraFile PreviewFile
	{
		get {
			return preview;
		}
		set {
			preview = value;
		}
	}
	
	public void ReleaseGPhotoResources ()
	{
		if (normal != null) 
			normal.Dispose ();

		if (preview != null) 
			preview.Dispose ();
	}

	public int CompareTo (object obj)
	{
		GPhotoCameraFile f2 = obj as GPhotoCameraFile;
		int result = Directory.CompareTo (f2.Directory);
		if (result == 0)
			result = FileName.CompareTo (f2.FileName);
		return result;
	}
}
