using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using WW.Cad.Drawing;
using WW.Cad.Drawing.GDI;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Math;


namespace CAD2CSV
{
    class VectorGraphicsDataInfoExtractor
    {
        private DxfModel                m_Model = null;
        private DrawContext.Wireframe   m_DrawContext = null;

        public void ParseFolder(string iInputPath, string iOutputCSVFilePath, string iExportedImageFormat)
        {
            List<string> dxf_and_dwg_files_paths = new List<string>(16);
            
            string[] filePaths = Directory.GetFiles(iInputPath, "*.dxf", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                dxf_and_dwg_files_paths.Add(filePaths[i]);
            }

            filePaths = Directory.GetFiles(iInputPath, "*.dgw", SearchOption.AllDirectories);
            for (int i = 0; i < filePaths.Length; i++)
            {
                dxf_and_dwg_files_paths.Add(filePaths[i]);
            }

            string imageoutputpath = Path.GetDirectoryName(iOutputCSVFilePath);

            DxfModel modeltoparse = null;
            StreamWriter csvfile =  new StreamWriter(iOutputCSVFilePath);
            Bounds3D loggedbounds = new Bounds3D();
            double totallength = 0.0;


            foreach (string filepath in dxf_and_dwg_files_paths)
            {
                //Open Model
                modeltoparse = GetDXFModelFromFile(filepath);
                if (modeltoparse != null)
                {
                    Console.WriteLine("Exporting: " + filepath);
                    //MinMax                
                    loggedbounds.Min = Point3D.NaN;
                    loggedbounds.Max = Point3D.NaN;
                    loggedbounds = ComputeDrawingMinMax(modeltoparse);

                    //Drawing total length
                    totallength = 0.0;
                    totallength = ComputeTotalLength(modeltoparse);

                    //Log retrieved data to CSV
                    LogToCSVFile(csvfile, filepath, loggedbounds, totallength);

                    //Export drawing to bitmap
                    ExportToBitmap(modeltoparse, filepath, imageoutputpath, iExportedImageFormat);
                }
            }
            csvfile.Close();
        }

        private DxfModel GetDXFModelFromFile(string iFile)
        {
            try
            {
                string extension = Path.GetExtension(iFile);
                if (string.Compare(extension, ".dwg", true) == 0)
                {
                    m_Model = DwgReader.Read(iFile);
                }
                else
                {
                    m_Model = DxfReader.Read(iFile);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occurred: " + e.Message);
                return null;
            }

            return m_Model;
        }

        private Bounds3D ComputeDrawingMinMax(DxfModel modeltoparse)
        {
            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds(modeltoparse);
            return boundsCalculator.Bounds;
        }

        private double ComputeTotalLength(DxfModel modeltoparse)
        {
            m_DrawContext = new DrawContext.Wireframe.ModelSpace(
                                modeltoparse,
                                GraphicsConfig.BlackBackgroundCorrectForBackColor,
                                Matrix4D.Identity
                                );
            double totallength = 0.0;

            if (modeltoparse.Blocks.Count != 0)
            {
                foreach (var block in modeltoparse.Blocks)
                {
                    totallength += GetEntitiesLength(block.Entities);
                }
            }
            else
            {
                totallength += GetEntitiesLength(modeltoparse.Entities);
            }
            return totallength;
        }

        private double GetEntitiesLength(DxfEntityCollection iEntities)
        {
            double entitieslength = 0.0;
            foreach (DxfEntity entity in iEntities)
            {
                LengthComputerGraphicsFactory lengthComputer = new LengthComputerGraphicsFactory();
                if (entity.EntityType == "INSERT")
                {
                    DxfInsert insert = (DxfInsert)entity;
                    entitieslength += GetEntitiesLength(insert.Block.Entities);
                }
                else
                {
                    entity.Draw(m_DrawContext, lengthComputer);
                    double length = lengthComputer.GetLength();
                    if (double.IsNaN(length))
                    {
                        Console.Error.WriteLine("Error occurred: Invalid length for entity "+ entity.Handle.ToString());
                    }
                    else
                    {
                        entitieslength += length;
                    }
                }
            }
            return entitieslength;
        }        

        private void ExportToBitmap(DxfModel iModeltoparse, string iFileToExportPath, string iOutputBitmapPath, string iExportedImageFormat)
        {
            GDIGraphics3D graphics = new GDIGraphics3D(GraphicsConfig.BlackBackgroundCorrectForBackColor);
            Size maxSize = new Size(500, 500);
            Bitmap bitmap =
                ImageExporter.CreateAutoSizedBitmap(
                    iModeltoparse,
                    graphics,
                    Matrix4D.Identity,
                    System.Drawing.Color.Black,
                    maxSize
                );
            
            Stream stream;
            string outfile = iOutputBitmapPath + "\\" + Path.GetFileNameWithoutExtension(Path.GetFullPath(iFileToExportPath));

            switch (iExportedImageFormat)
            {
                case "bmp":
                    using (stream = File.Create(outfile + ".bmp"))
                    {
                        ImageExporter.EncodeImageToBmp(bitmap, stream);
                    }
                    break;
                case "gif":
                    using (stream = File.Create(outfile + ".gif"))
                    {
                        ImageExporter.EncodeImageToGif(bitmap, stream);
                    }
                    break;
                case "tiff":
                    using (stream = File.Create(outfile + ".tiff"))
                    {
                        ImageExporter.EncodeImageToTiff(bitmap, stream);
                    }
                    break;
                case "png":
                    using (stream = File.Create(outfile + ".png"))
                    {
                        ImageExporter.EncodeImageToPng(bitmap, stream);
                    }
                    break;
                case "jpg":
                    using (stream = File.Create(outfile + ".jpg"))
                    {
                        ImageExporter.EncodeImageToJpeg(bitmap, stream);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown format " + iExportedImageFormat + ".");
                    break;
            }
        }

        private void LogToCSVFile(StreamWriter iCSVFile, string iFilePath, Bounds3D iLoggedbounds, double iTotalLength)
        {
            string line = iFilePath + ";" + iLoggedbounds.ToString() + ";" + iTotalLength.ToString();
            iCSVFile.WriteLine(line);
        }
    }
}