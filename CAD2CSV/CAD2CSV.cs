using System;
using System.IO;


namespace CAD2CSV
{
    class CAD2CSV
    {
        static void Main(string[] args)
        {
            //Check number of command line parameters 
            if (args.Length != 3)
            {
                Console.Error.WriteLine("Error occurred: incorrect number of parameters\nUsage: cad2csv <input folder> <output csv file absolute path> <exportedimagefileformat> \n Valid Exported Image File Format: bmp gif tiff png jpg");
                return;
            }

            //Check input path validity
            //TODO check trailing \
            if (System.IO.Directory.Exists(args[0]) == false)
            {
                Console.Error.WriteLine("Error occurred: input folder does not exist");
                return;
            }

            //Check output path validity
            //TODO check trailing \

            string outputdirectory = Path.GetDirectoryName(args[1]);
            if (Directory.Exists(outputdirectory) == false)
            {
                Console.Error.WriteLine("Error occurred: output folder does not exist");
                return;
            }

            //Check exported image file format
            string[] validfileformats = {"bmp", "gif", "tiff", "png", "jpg"};
            bool valid = false;
            for (int i = 0; i < validfileformats.Length; i++)
            {
                if (args[2] == validfileformats[i])
                {
                    valid = true;
                }
            }
            if (valid == false)
            {
                Console.Error.WriteLine("Error occurred: Invalid exported image file format.\n Valide format are: bmp gif tiff png jpg");
                return;
            }

            VectorGraphicsDataInfoExtractor vgdie = new VectorGraphicsDataInfoExtractor();
            vgdie.ParseFolder(args[0], args[1], args[2]);
        }        
    }
}