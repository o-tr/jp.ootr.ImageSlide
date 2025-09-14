import sharp from "sharp";
import * as fs from "fs/promises";
import * as path from "path";

const __dirname = import.meta.dirname;
const __projectRoot = path.join(__dirname, "../../../../");

const packageJson = (async()=>{
    let pkgText = await fs.readFile(path.join(__projectRoot, "package.json"), 'utf-8');
    if (pkgText.charCodeAt(0) === 0xFEFF) {
        pkgText = pkgText.slice(1);
    }
    return JSON.parse(pkgText) as {version: string};
})()

const generatePng = async(svgPath: string, outputPath: string) => {
    const svgText = await fs.readFile(svgPath, 'utf-8');

    const versionedSvgText = svgText.replace(/\[version]/g, `v${(await packageJson).version}`);

    await sharp(Buffer.from(versionedSvgText, 'utf-8')).png().toFile(outputPath);
}

(async()=>{
    await Promise.all([
        generatePng(path.join(__dirname,"../assets/ImageSlide.svg"), path.join(__projectRoot,"Assets/Textures/SplashScreen/ImageSlide.png")),
        generatePng(path.join(__dirname,"../assets/ImageSlideViewer.svg"), path.join(__projectRoot,"Assets/Textures/SplashScreen/ImageSlideViewer.png")),
    ]);
})()
