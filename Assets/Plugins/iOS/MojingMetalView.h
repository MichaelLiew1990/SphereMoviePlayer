//
//  MojingMetalView.h
//  MojingSDK
//
//  Created by Tracy Ma on 16/6/15.
//  Copyright © 2016年 baofeng. All rights reserved.
//

#ifndef MojingMetalView_h
#define MojingMetalView_h

#include <TargetConditionals.h>
#if (TARGET_CPU_ARM || TARGET_CPU_ARM64)

#import <QuartzCore/CAMetalLayer.h>
#import <Metal/Metal.h>
#import <UIKit/UIKit.h>

@protocol AAPLViewDelegate;

@interface MojingMetalView : UIView
@property (nonatomic, weak) IBOutlet id <AAPLViewDelegate> delegate;

// view has a handle to the metal device when created
@property (nonatomic, readonly) id <MTLDevice> device;

// the current drawable created within the view's CAMetalLayer
@property (nonatomic, readonly) id <CAMetalDrawable> currentDrawable;

// The current framebuffer can be read by delegate during -[MetalViewDelegate render:]
// This call may block until the framebuffer is available.
@property (nonatomic, readonly) MTLRenderPassDescriptor *renderPassDescriptor;

// set these pixel formats to have the main drawable framebuffer get created with depth and/or stencil attachments
@property (nonatomic) MTLPixelFormat depthPixelFormat;
@property (nonatomic) MTLPixelFormat stencilPixelFormat;
@property (nonatomic) NSUInteger     sampleCount;

// view controller will be call off the main thread
- (void)display;

// release any color/depth/stencil resources. view controller will call when paused.
- (void)releaseTextures;

@end

// rendering delegate (App must implement a rendering delegate that responds to these messages
@protocol AAPLViewDelegate <NSObject>

@required
// called if the view changes orientation or size, renderer can precompute its view and projection matricies here for example
- (void)reshape:(MojingMetalView *)view;

// delegate should perform all rendering here
- (void)render:(MojingMetalView *)view;

- (void)startFrame:(MojingMetalView *)view;
- (void)drawMesh;
- (void)endFrame;

@end

#endif /* MJ_IOS_METAL */
#endif /* MojingMetalView_h */
