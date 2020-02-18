var mkdirp = require('mkdirp');
var path = require('path');
var ncp = require('ncp');

let packageName = '';
let args = process.argv.slice(2);

// Package name
if (args[0]) {
  packageName = args[0];
}
else {
  throw new Error("Missing arg to postinstall.js in package.json.");
}

// Paths
var src = path.join(__dirname, '..', 'Assets', packageName);
var dir = path.join(__dirname, '..', '..', '..', 'Assets', 'pkg-all', packageName);

// Create folder if missing
mkdirp(dir, function (err) {
  if (err) {
    console.error(err)
    process.exit(1);
  }

  // Copy files
  ncp(src, dir, function (err) {
    if (err) {
      console.error(err);
      process.exit(1);
    }
  });
});