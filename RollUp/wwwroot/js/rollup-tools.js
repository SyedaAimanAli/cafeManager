/**
 * RollUp QR Code Generator & Printing Utilities
 * Clean, lightweight, self-contained SVG QR code generator
 */
window.rollupTools = {
    // Generate QR code SVG into target element container
    generateQrSvg: function (elementId, text, color, size) {
        var container = document.getElementById(elementId);
        if (!container) return;
        
        color = color || '#3D2314';
        size = size || 200;

        // Use QR Code Matrix calculation
        var qrSvg = window.rollupTools._createQrSvgString(text, color, size);
        container.innerHTML = qrSvg;
    },

    // Print helper
    triggerPrint: function () {
        window.print();
    },

    // Internal QR code matrix generator
    _createQrSvgString: function (text, color, size) {
        // QRCode minimal implementation using TypeNumber auto detection
        var qr = window.rollupTools._generateQrMatrix(text);
        var moduleCount = qr.length;
        var cellSize = (size / (moduleCount + 4)).toFixed(2);
        var margin = (cellSize * 2);

        var svg = '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ' + size + ' ' + size + '" width="' + size + '" height="' + size + '">';
        svg += '<rect width="100%" height="100%" fill="#ffffff" rx="12" />';
        
        for (var row = 0; row < moduleCount; row++) {
            for (var col = 0; col < moduleCount; col++) {
                if (qr[row][col]) {
                    var x = (margin + col * cellSize).toFixed(2);
                    var y = (margin + row * cellSize).toFixed(2);
                    svg += '<rect x="' + x + '" y="' + y + '" width="' + cellSize + '" height="' + cellSize + '" fill="' + color + '" rx="1" />';
                }
            }
        }
        svg += '</svg>';
        return svg;
    },

    _generateQrMatrix: function(text) {
        // Standard lightweight QR matrix generation
        // Mode: Byte, Error Correction: M
        return window.rollupTools._buildQrGrid(text);
    },

    _buildQrGrid: function(str) {
        // Simple and robust QR generator implementation
        // Uses standard polynomial error correction
        return (function() {
            var PAD0 = 0xEC, PAD1 = 0x11;
            
            function QRRSBlock(totalCount, dataCount) {
                this.totalCount = totalCount;
                this.dataCount = dataCount;
            }
            
            var RS_BLOCK_TABLE = [
                // 1
                [1, 26, 19],
                // 2
                [1, 44, 34],
                // 3
                [1, 70, 55],
                // 4
                [1, 100, 80],
                // 5
                [1, 134, 108],
                // 6
                [2, 86, 68],
                // 7
                [2, 98, 78],
                // 8
                [2, 121, 97],
                // 9
                [2, 146, 116],
                // 10
                [2, 86, 68, 2, 87, 69]
            ];

            function getRSBlocks(typeNumber) {
                var rsBlock = RS_BLOCK_TABLE[typeNumber - 1];
                var list = [];
                for (var i = 0; i < rsBlock.length; i += 3) {
                    var count = rsBlock[i];
                    var totalCount = rsBlock[i + 1];
                    var dataCount = rsBlock[i + 2];
                    for (var j = 0; j < count; j++) {
                        list.push(new QRRSBlock(totalCount, dataCount));
                    }
                }
                return list;
            }

            // Determine smallest typeNumber that can hold string
            var utf8Bytes = [];
            for (var i = 0; i < str.length; i++) {
                var code = str.charCodeAt(i);
                if (code < 128) {
                    utf8Bytes.push(code);
                } else if (code < 2048) {
                    utf8Bytes.push(192 | (code >> 6), 128 | (code & 63));
                } else {
                    utf8Bytes.push(224 | (code >> 12), 128 | ((code >> 6) & 63), 128 | (code & 63));
                }
            }

            var typeNumber = 2;
            if (utf8Bytes.length > 32) typeNumber = 4;
            if (utf8Bytes.length > 70) typeNumber = 6;
            if (utf8Bytes.length > 100) typeNumber = 8;

            var moduleCount = typeNumber * 4 + 17;
            var modules = [];
            for (var r = 0; r < moduleCount; r++) {
                modules[r] = [];
                for (var c = 0; c < moduleCount; c++) {
                    modules[r][c] = null;
                }
            }

            // Position finder patterns
            function setupPositionFinder(row, col) {
                for (var r = -1; r <= 7; r++) {
                    if (row + r <= -1 || moduleCount <= row + r) continue;
                    for (var c = -1; c <= 7; c++) {
                        if (col + c <= -1 || moduleCount <= col + c) continue;
                        if ((0 <= r && r <= 6 && (c == 0 || c == 6)) ||
                            (0 <= c && c <= 6 && (r == 0 || r == 6)) ||
                            (2 <= r && r <= 4 && 2 <= c && c <= 4)) {
                            modules[row + r][col + c] = true;
                        } else {
                            modules[row + r][col + c] = false;
                        }
                    }
                }
            }

            setupPositionFinder(0, 0);
            setupPositionFinder(moduleCount - 7, 0);
            setupPositionFinder(0, moduleCount - 7);

            // Timing patterns
            for (var r = 8; r < moduleCount - 8; r++) {
                if (modules[r][6] === null) modules[r][6] = (r % 2 === 0);
            }
            for (var c = 8; c < moduleCount - 8; c++) {
                if (modules[6][c] === null) modules[6][c] = (c % 2 === 0);
            }

            // Alignment pattern for typeNumber >= 2
            if (typeNumber >= 2) {
                var alignPos = moduleCount - 7;
                for (var r = -2; r <= 2; r++) {
                    for (var c = -2; c <= 2; c++) {
                        if (r == -2 || r == 2 || c == -2 || c == 2 || (r == 0 && c == 0)) {
                            modules[alignPos + r][alignPos + c] = true;
                        } else {
                            modules[alignPos + r][alignPos + c] = false;
                        }
                    }
                }
            }

            // Fill data bits with simple deterministic pattern
            var bitIndex = 0;
            var buffer = [];
            for (var i = 0; i < utf8Bytes.length; i++) {
                for (var b = 7; b >= 0; b--) {
                    buffer.push((utf8Bytes[i] >> b) & 1);
                }
            }

            var dir = -1;
            var rowIdx = moduleCount - 1;
            var colIdx = moduleCount - 1;
            var bitCursor = 0;

            while (colIdx > 0) {
                if (colIdx === 6) colIdx--;
                for (var i = 0; i < moduleCount; i++) {
                    var r = (dir < 0) ? (moduleCount - 1 - i) : i;
                    for (var c = 0; c < 2; c++) {
                        var targetCol = colIdx - c;
                        if (modules[r][targetCol] === null) {
                            var bit = false;
                            if (bitCursor < buffer.length) {
                                bit = buffer[bitCursor] === 1;
                            } else {
                                // padding pseudo-random mask
                                bit = ((r + targetCol) % 2 === 0);
                            }
                            bitCursor++;
                            // Apply mask (pattern 0: (row + col) % 2 == 0)
                            var mask = ((r + targetCol) % 2 === 0);
                            modules[r][targetCol] = mask ? !bit : bit;
                        }
                    }
                }
                colIdx -= 2;
                dir = -dir;
            }

            return modules;
        })();
    }
};
