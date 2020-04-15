using UnityEngine;
using RootUtils.Randomization;

public static class HexagonConstants {

/// <summary>
/// The ratio of the outer radius of a hexagon to the inner radius.
/// </summary>
// Take one of the six triangles of a hexagon. The inner radius is equal to the
// height of this triangle. You get this height by splitting the triangle into
// two right triangles, then you can use the Pythagorean theorem . . .
//      
// . . . a^2 * b^2 = hypotenuse^2, or . . .
//          
// . . . (len_adjacent^2 * len_opposite^2 = len_hypotenuse^2) . . .
//
// . . . to derive the following equation . . .
//
//          let e be edge length
//          let ir be inner radius
//
//          ir = sqrt(e^2 - (e/2)^2 = sqrt(3(e^2/4)) = e(sqrt(3)/2) ~= 0.886e
//
//                                    [outer radius] (1)
//                             ,//*.   ,@@ /,@@    ,//*                           
//                        //       #@(     /     @@.      ,/*                    
//                    /*       @@%         /       *#%@(       /*                 
//                 /.     *@&              /             *@&      /*              
//              */    %@%   .#             /            .#    @@,    /            
//            **  @@.         *            /           (         ##@(  /          
//          ,#@&                ,          /                       #  ,@@/        
//         /@*/.                           /  [inner radius] (~0.886) # */*@,      
//        /.@  ..,/,                       /                       // .  ,&,*     
//       / .@ #       //            *      /      *           ,/*        ,& ,,    
//      /  .@#.           */.        (.    /    (         */.           .,&  *.   
//     .*  .@*                ,/*      #   /  .#      //                 %&   /   
//     /   .@                      //   #, / #.  ,/*                     (&   *.  
//     /   .@                          */,#(#*/.                         ,&   ,,  
//     /   .@                           ./##(*                           ,&   ,,  
//     /   .@                       ,/, ./ / (  //                       /&   *.  
//     ,,  .@.                  */.    ,   /        */.                  *&   /   
//      /  .@              ./*             /            ./*              ,&  ,,   
//      .* .@          ,/,           .     /                 //         (,&  /    
//       ,*.@ .(   */.             ,       /      *              */.  .# ,& /     
//        ./@ ./#                .(        /        (                #(* ,@/      
//          @@,  .#             #.         /         (,            *#   #@(       
//           ./ (@(,#         ,#           /          .#         /#.@@../         
//             ,/   ,@@*     #,            /            #.     %@%   ./           
//                /.     &@*#              /              ##@/     /*             
//                  ./.      (@%           /          ,@@.      **                
//                      */.      ,@&,, ## ## (#..*/@%       */.                   
//                           */*.     %@*  /  %@/     ,//,                        
//                                   .,**/%@/***,.
//
// . . . consequently, the ratio of the outer radius to the inner raidus is
//       ir if the outer radius were 1.     
    public const float OUTER_TO_INNER_RATIO = 0.866025404f;

/// <summary>
/// The ratio of the inner radius of a hexagon to the outer radius.
/// </summary>
/// 1f / OuterToInnerRatio;
    public const float INNER_TO_OUTER_RATIO = 1.15470053809183639144f;

/// <summary>
/// An array of points corresponding to the corners of a hexagon in
/// clockwise order.
/// </summary>
/// <value></value>
    public static readonly Vector3[] REFERENCE_CORNERS = {
// North
        new Vector3(0f, 0f, 1f),
// Northeast
        new Vector3(OUTER_TO_INNER_RATIO, 0f, 0.5f),
// Southeast
        new Vector3(OUTER_TO_INNER_RATIO, 0f, -0.5f),
// South
        new Vector3(0f, 0f, -1f),
// Southwest
        new Vector3(-OUTER_TO_INNER_RATIO, 0f, -0.5f),
// Northwest
        new Vector3(-OUTER_TO_INNER_RATIO, 0f, 0.5f)
    };
}

